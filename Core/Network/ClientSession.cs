using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Gem.Network
{
    public class Peer
    {
        public Guid Guid { get; internal set; }
        public Object tag = null;
    }

    public class ClientSession
    {
        public Action onLostConnection = null;
        public Action<Peer> onPeerJoined = null;
        public Action<Peer> onPeerLeft = null;
        public Action<byte[]> onDatagramReceived = null;
        public Action<String> debugOutput = null;
        public bool debug = false;

        internal IPEndPoint serverAddress;
        
        public uint millisecondsBeforeRetry = 1000; //Try to send packets every half second until receipt acknowledged.
        public uint retryAttempts = 20; //How many times to try sending a critical packet before giving up.

        private List<Peer> peers = new List<Peer>();
        private UdpClient socket;
        private uint nextAckID = 1;
        private List<CriticalDatagram> criticalDatagrams = new List<CriticalDatagram>();
        private RemotePeer duplicateDatagramDetector = new RemotePeer();

        private void _onLostConnection()
        {
            if (onLostConnection != null) onLostConnection();
        }

        private void _debug(String msg)
        {
            if (debug && debugOutput != null)
            {
                try { debugOutput(msg); }
                catch (Exception) { }
            }
        }

        private void _sendCriticalDatagram(byte[] data, uint ackID)
        {
            var criticalDatagram = new CriticalDatagram
            {
                data = data,
                ackID = ackID,
                lastSendAttempt = DateTime.MinValue,
                sendAttempts = 0,
            };
            criticalDatagrams.Add(criticalDatagram);
        }

        public Peer findPeer(Guid guid)
        {
            return peers.FirstOrDefault((peer) => { return peer.Guid == guid; });
        }
       
        private bool _send(byte[] data)
        {
            try
            {
                socket.Send(data, data.Length, serverAddress);
                return true;
            }
            catch (SystemException)
            {
                _onLostConnection();
                return false;
            }
        }

        private void _sendAckMessage(uint ackID)
        {
            var ackDatagram = new WriteOnlyDatagram();
            ackDatagram.WriteUInt((uint)ClientToServerMessage.Acknowledge, 8);
            ackDatagram.WriteUInt(ackID, 32);
            _send(ackDatagram.BufferAsArray);
        }


        public void update()
        {
            try
            {
                while (socket.Available > 0)
                {
                    IPEndPoint observedEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] bytes = null;
                    try
                    {
                        bytes = socket.Receive(ref observedEndpoint);
                    }
                    catch (System.Net.Sockets.SocketException except)
                    {
                        _debug("[CS] Receive threw an exception. Socket Error : " + except.SocketErrorCode);
                        onLostConnection();
                        continue;
                    }

                    //_debug("[CS] Incoming message of length " + bytes.Length.ToString() + " : " + BitConverter.ToString(bytes));
                    //_debug("[CS] It appears to have come from " + observedEndpoint.ToString());

                    if (!observedEndpoint.Equals(serverAddress))
                    {
                        _debug("[CS] I don't know who this message is from.");
                        continue;
                    }

                    var datagram = new ReadOnlyDatagram(bytes);
                    uint messageType = 0;
                    uint ackID = 0;
                    if (!datagram.ReadUInt(out messageType, 8)) goto OnBadMessage;
                    if (!datagram.ReadUInt(out ackID, 32)) goto OnBadMessage;

                    if (ackID != 0) _sendAckMessage(ackID);
                    if (messageType != (uint)ServerToClientMessage.Acknowledge && 
                        ackID != 0 && 
                        duplicateDatagramDetector.newDatagramReceived(ackID) == RemotePeer.DatagramResponse.Ignore)
                        continue;

                    switch ((ServerToClientMessage)messageType)
                    {
                        case ServerToClientMessage.Acknowledge:
                            _removeCriticalDatagram(ackID);
                            //if (ackID == 1) _debug("Received ACK for join");
                            break;
                        case ServerToClientMessage.Keepalive:
                            {
                                //Ack response already handled
                            }
                            break;
                        case ServerToClientMessage.PeerJoined:
                            {
                                var guidBytes = new byte[16];
                                if (!datagram.ReadBytes(guidBytes, 16)) goto OnBadMessage;
                                var peerGuid = new Guid(guidBytes);
                                var newPeer = new Peer { Guid = peerGuid };
                                if (onPeerJoined != null) onPeerJoined(newPeer);
                                peers.Add(newPeer);
                            }
                            break;
                        case ServerToClientMessage.PeerLeft:
                            {
                                var guidBytes = new byte[16];
                                if (!datagram.ReadBytes(guidBytes, 16)) goto OnBadMessage;
                                var peerGuid = new Guid(guidBytes);
                                var peer = findPeer(peerGuid);
                                if (onPeerLeft != null) onPeerLeft(peer);
                                peers.Remove(peer);
                            }
                            break;
                        case ServerToClientMessage.Datagram:
                            {
                                //_debug("[CS] I received a datagram from the server. It was " + (bytes.Length - 5).ToString() + " bytes long.");
                                if (bytes.Length - 5 <= 0) goto OnBadMessage;
                                byte[] data = new byte[bytes.Length - 5];
                                if (!datagram.ReadBytes(data, (uint)(bytes.Length - 5))) goto OnBadMessage;
                                if (onDatagramReceived != null)
                                {
                                    try { onDatagramReceived(data); }
                                    catch (Exception) { }
                                }
                                break;
                            }
                    }
                    continue;
                OnBadMessage:
                    {
                        _debug("[CS] I received a bad message.");
                    }
                }

                var now = DateTime.Now;
                for (int i = 0; i < criticalDatagrams.Count; )
                {
                    var criticalDatagram = criticalDatagrams[i];
                    var timeDelta = now - criticalDatagram.lastSendAttempt;
                    if (timeDelta.TotalMilliseconds > millisecondsBeforeRetry)
                    {
                        if (criticalDatagram.sendAttempts >= retryAttempts)
                        {
                            _debug("[CS] I was not able to deliver critical packet " + criticalDatagram.ackID + ".");
                            criticalDatagrams.RemoveAt(i);
                            _onLostConnection();
                            continue;
                        }

                        criticalDatagram.sendAttempts += 1;
                        //_debug("[CS] I am trying to send critical packet " + criticalDatagram.ackID + ". ["
                        //    + criticalDatagram.sendAttempts + " attempts]");
                        criticalDatagram.lastSendAttempt = now;
                        _send(criticalDatagram.data);
                    }

                    ++i;
                }
            }
            catch (Exception e) 
            { 
                _debug("[CS] " + e.Message + "\n" + e.StackTrace); 
            }
        }

        private void _removeCriticalDatagram(uint ackID)
        {
            var criticalDatagram = criticalDatagrams.FirstOrDefault((datagram) => { return datagram.ackID == ackID; });
            if (criticalDatagram != null)
                criticalDatagrams.Remove(criticalDatagram);
        }

        public ClientSession(int LocalPort, IPEndPoint serverAddress, Action<String> debugOutput) 
        {
            this.debugOutput = debugOutput;

            socket = new UdpClient(LocalPort, AddressFamily.InterNetwork);
            socket.DontFragment = true;
            this.serverAddress = serverAddress;

            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ClientToServerMessage.Join, 8);
            var ackID = nextAckID++;
            datagram.WriteUInt(ackID, 32);
            _sendCriticalDatagram(datagram.BufferAsArray, ackID);
        }

        public int peerCount { get { return peers.Count; } }
        public Peer getPeer(int i) { return peers[i]; }

        public void foreachPeer(Action<Peer> task)
        {
            peers.ForEach(task);
        }

        public void sendCriticalDatagram(byte[] data)
        {
            var ackID = nextAckID++;
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ClientToServerMessage.Datagram, 8);
            datagram.WriteUInt(ackID, 32);
            datagram.WriteBytes(data);
            _sendCriticalDatagram(datagram.BufferAsArray, ackID);
        }

        public void sendDatagram(byte[] data)
        {
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ClientToServerMessage.Datagram, 8);
            datagram.WriteUInt(0, 32);
            datagram.WriteBytes(data);
            _send(datagram.BufferAsArray);
        }
    }
}
