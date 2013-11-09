using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Gem.Network
{
    public class Client : RemotePeer
    {
        public IPEndPoint observedAddress { get; set; }
        public DateTime lastCommunication { get; set; }
        public Guid Guid { get; set; }
        public Object tag { get; set; }
    }

    public class ServerSession
    {
        public Action<Client> onClientJoined = null;
        public Action<Client> onClientLeft = null;
        public Action<Client, byte[]> onDatagramReceived = null;
        public Action<int, String> debugOutput = null;
        public bool debug = false;
        
        public uint millisecondsBeforeRetry = 1000; //Try to send packets every half second until receipt acknowledged.
        public uint retryAttempts = 20; //How many times to try sending a critical packet before giving up.
        public uint keepaliveRate = 30000; //Milliseconds between keepalive signals.

        private List<Client> clients = new List<Client>();
        private UdpClient socket;
        private uint nextAckID = 1;
        private List<CriticalDatagram> criticalDatagrams = new List<CriticalDatagram>();

        private void _debug(int level, String msg)
        {
            if (debug && debugOutput != null) debugOutput(level, msg);
        }

        private void _onLostClient(Client client)
        {
            clients.Remove(client);

            foreach (var existingClient in clients)
            {
                _sendPeerLeftMessage(existingClient, client);
            }
            if (onClientLeft != null) onClientLeft(client);
        }

        private void _sendCriticalDatagram(Client to, byte[] data, uint ackID, Action onSuccess = null)
        {
            var criticalDatagram = new CriticalDatagram
            {
                to = to,
                data = data,
                ackID = ackID,
                lastSendAttempt = System.DateTime.MinValue,
                sendAttempts = 0,
                onSuccess = onSuccess,
            };
            criticalDatagrams.Add(criticalDatagram);
        }

        private void _sendAckMessage(Client to, uint ackID)
        {
            var  ackDatagram = new WriteOnlyDatagram();
            ackDatagram.WriteUInt((uint)ServerToClientMessage.Acknowledge, 8);
            ackDatagram.WriteUInt(ackID, 32);
            _send(ackDatagram.BufferAsArray, to);
        }

        private void _send(byte[] data, Client to)
        {
            socket.Send(data, data.Length, to.observedAddress);
            to.lastCommunication = DateTime.Now;
        }

        private void _sendPeerJoinedMessage(Client to, Client about)
        {
            var ackID = nextAckID++;
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ServerToClientMessage.PeerJoined, 8);
            datagram.WriteUInt((uint)ackID, 32);
            datagram.WriteBytes(about.Guid.ToByteArray());
            _sendCriticalDatagram(to, datagram.BufferAsArray, ackID);
        }

        private void _sendPeerLeftMessage(Client to, Client about)
        {
            var ackID = nextAckID++;
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ServerToClientMessage.PeerLeft, 8);
            datagram.WriteUInt((uint)ackID, 32);
            datagram.WriteBytes(about.Guid.ToByteArray());
            _sendCriticalDatagram(to, datagram.BufferAsArray, ackID);
        }

        private void _sendKeepalive(Client to)
        {
            var ackID = nextAckID++;
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ServerToClientMessage.Keepalive, 8);
            datagram.WriteUInt((uint)ackID, 32);
            _sendCriticalDatagram(to, datagram.BufferAsArray, ackID);
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
                        _debug(0, "[SS] Receive threw an exception. Socket Error : " + except.SocketErrorCode);
                        continue;
                    }

                    _debug(1, "[SS] Incoming message of length " + bytes.Length.ToString() + " : " + BitConverter.ToString(bytes));
                    _debug(1, "[SS] It appears to have come from " + observedEndpoint.ToString());

                    var datagram = new ReadOnlyDatagram(bytes);
                    uint messageType = 0;
                    uint ackID = 0;
                    if (!datagram.ReadUInt(out messageType, 8)) goto OnBadMessage;
                    if (!datagram.ReadUInt(out ackID, 32)) goto OnBadMessage;
                    var sendingClient = clients.FirstOrDefault((client) => { return client.observedAddress.Equals(observedEndpoint); });
                    
                    switch ((ClientToServerMessage)messageType)
                    {
                        case ClientToServerMessage.Join:
                            {
                                if (sendingClient != null) //Client has already joined - assume the ack was dropped.
                                {
                                    _sendAckMessage(sendingClient, ackID);
                                    break;
                                }

                                try
                                {
                                    var newClient = new Client();
                                    newClient.observedAddress = observedEndpoint;
                                    newClient.Guid = Guid.NewGuid();
                                    newClient.lastCommunication = DateTime.Now;


                                    _debug(0, "[SS] A client with observed address " + observedEndpoint.ToString() + " has joined the session.");

                                    foreach (var existingClient in clients)
                                    {
                                        _sendPeerJoinedMessage(existingClient, newClient);
                                        _sendPeerJoinedMessage(newClient, existingClient);
                                    }

                                    clients.Add(newClient);
                                    if (onClientJoined != null) onClientJoined(newClient);

                                    _sendAckMessage(newClient, ackID);
                                }
                                catch (Exception e)
                                {
                                    _debug(0, "[SS] Accepting new client threw exception: " + e.Message);
                                }
                            }
                            break;
                        case ClientToServerMessage.Acknowledge:
                            {
                                var criticalDatagram = criticalDatagrams.FirstOrDefault((crit) => { return crit.ackID == ackID; });
                                if (criticalDatagram != null)
                                {
                                    if (!criticalDatagram.to.observedAddress.Equals(observedEndpoint))
                                    {
                                        _debug(1, "[SS] The observed endpoint of the ack reply does not match the known observed endpoint of the peer the datagram was sent to.");
                                        break;
                                    }
                                    if (criticalDatagram.onSuccess != null)
                                    {
                                        try
                                        {
                                            criticalDatagram.onSuccess();
                                        }
                                        catch (Exception) { }
                                    }
                                    criticalDatagrams.Remove(criticalDatagram);
                                }
                            }
                            break;
                       case ClientToServerMessage.Datagram:
                            {
                                if (sendingClient == null)
                                {
                                    //_debug("[SS] I received a datagram from an unknown client with address " + observedEndpoint.ToString() +".");
                                    break;
                                }

                                if (ackID != 0)
                                {
                                    _sendAckMessage(sendingClient, ackID);
                                    if (sendingClient.newDatagramReceived(ackID) == RemotePeer.DatagramResponse.Ignore)
                                        break;
                                }

                                _debug(2, "[SS] I received a datagram from client " + sendingClient.Guid.ToString()
                                    + ". It was " + (bytes.Length - 5).ToString() + " bytes long.");
                                if (bytes.Length - 5 <= 0) goto OnBadMessage;
                                byte[] data = new byte[bytes.Length - 5];
                                if (!datagram.ReadBytes(data, (uint)(bytes.Length - 5))) goto OnBadMessage;
                                if (onDatagramReceived != null) onDatagramReceived(sendingClient, data); 
                            }
                            break;
                    }
                    continue;
                OnBadMessage:
                    {
                        _debug(1, "[SS] I received a bad message.");
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
                            _debug(0, "[SS] I was not able to deliver critical packet " + criticalDatagram.ackID + ".");
                            criticalDatagrams.RemoveAt(i);
                            _onLostClient(criticalDatagram.to);
                            continue;
                        }

                        criticalDatagram.sendAttempts += 1;
                        _debug(2, "[SS] I am trying to send critical packet " + criticalDatagram.ackID + ". [" + 
                            criticalDatagram.sendAttempts + " attempts]");
                        criticalDatagram.lastSendAttempt = now;
                        _send(criticalDatagram.data, criticalDatagram.to);
                    }

                    ++i;
                }

                foreach (var client in clients)
                {
                    var timeDelta = now - client.lastCommunication;
                    if (timeDelta.TotalMilliseconds > keepaliveRate)
                        _sendKeepalive(client);
                }
            }
            catch (Exception e) { _debug(0, e.Message + "\n" + e.StackTrace); }
        }

        public void dropClient(Client who)
        {
            _debug(0, "[SS] Dropping client " + who.Guid.ToString());
            _onLostClient(who);
        }

        private void _removeCriticalDatagram(uint ackID)
        {
            var criticalDatagram = criticalDatagrams.FirstOrDefault((datagram) => { return datagram.ackID == ackID; });
            if (criticalDatagram != null)
                criticalDatagrams.Remove(criticalDatagram);
        }

        public ServerSession(int LocalPort, Action<int, String> debugOutput) 
        {
            this.debugOutput = debugOutput;

            socket = new UdpClient(LocalPort, AddressFamily.InterNetwork);
            socket.DontFragment = true;
            _debug(0, "[SS] Started server session on port " + LocalPort);
        }

        public int clientCount { get { return clients.Count; } }
        public Client getPeer(int i) { return clients[i]; }

        public void foreachClient(Action<Client> task)
        {
            clients.ForEach(task);
        }

        public void sendCriticalDatagram(Client to, byte[] data, Action onSuccess = null)
        {
            var ackID = nextAckID++;
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ServerToClientMessage.Datagram, 8);
            datagram.WriteUInt(ackID, 32);
            datagram.WriteBytes(data);
            _sendCriticalDatagram(to, datagram.BufferAsArray, ackID, onSuccess);
        }

        public void broadcastCriticalDatagram(byte[] data, Action onSuccess = null)
        {
            foreach (var client in clients)
                sendCriticalDatagram(client, data, onSuccess);
        }

        public void sendDatagram(Client to, byte[] data)
        {
            var datagram = new WriteOnlyDatagram();
            datagram.WriteUInt((uint)ServerToClientMessage.Datagram, 8);
            datagram.WriteUInt(0, 32);
            datagram.WriteBytes(data);
            _send(datagram.BufferAsArray, to);
        }

        public void broadcastDatagram(byte[] data)
        {
            foreach (var client in clients)
                sendDatagram(client, data);
        }
    }
}
