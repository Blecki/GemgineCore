using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Renderer;
using Gem.Common;
using Gem.Gui;

namespace Gem.Network
{
    public class ServerModule : IModule
    {
        public Network.ServerSession netSession;
        private int port;
        private float tickSeconds = 0.0f;
        private Simulation sim;
        private List<ISyncable> syncables = new List<ISyncable>();
        private List<byte[]> pendingMessages = new List<byte[]>();

        public ServerModule(int port)
        {
            this.port = port;
        }

        void IModule.BeginSimulation(Simulation sim)
        {
            this.sim = sim;
            sim.sendMessageHandler += (bytes) => { pendingMessages.Add(bytes); };

            try
            {
                netSession = new Network.ServerSession(port, sim.debug);

                netSession.onClientJoined += (client) =>
                {
                    var welcomeDatagram = new Network.WriteOnlyDatagram();
                    welcomeDatagram.WriteUInt(0, 8);
                    welcomeDatagram.WriteString(sim.Content.Module.Name);
                    welcomeDatagram.WriteUInt((uint)sim.Content.Module.Version, 32);
                    netSession.sendCriticalDatagram(client, welcomeDatagram.BufferAsArray, () =>
                    {
                        sim.EnqueueEvent("on-new-client", new MISP.ScriptList(client));
                    });
                };

                netSession.onDatagramReceived += (client, bytes) =>
                    {
                        var gram = new Network.ReadOnlyDatagram(bytes);
                        while (gram.More)
                        {
                            uint messageCode = 0;
                            gram.ReadUInt(out messageCode, 8);

                            switch (messageCode)
                            {
                                case 0:
                                    //Should never receive this message.
                                    break;
                                case 1:
                                    //Should never receive this message.
                                    break;
                                case 2:
                                    {
                                        UInt32 length;
                                        gram.ReadUInt(out length, 32);
                                        byte[] message = new byte[length];
                                        gram.ReadBytes(message, length);
                                        string messageID = null;
                                        MISP.ScriptList messageData = null;
                                        ScriptMessage.DecodeMessage(message, out messageID, out messageData);
                                        sim.EnqueueEvent(messageID, messageData);
                                    }
                                    break;
                            }
                        }
                    };
            }
            catch (Exception e)
            {
                System.Console.WriteLine("While trying to create a server module, " + e.Message);
                throw e;
            }
        }

        void IModule.Update(float elapsedSeconds)
        {
            netSession.update();
         
            var rawTickInterval = sim.settings.GetLocalProperty("tick-interval");
            float tickInterval = 1.0f;
            if (rawTickInterval != null) try { tickInterval = Convert.ToSingle(rawTickInterval); }
                catch (Exception) { };
            if (tickInterval <= 0) tickInterval = 1.0f;

            tickSeconds += elapsedSeconds;
            if (tickSeconds > tickInterval)
            {
                tickSeconds -= tickInterval;
                var datagram = new Network.WriteOnlyDatagram();
                
                foreach (var message in pendingMessages)
                {
                    datagram.WriteUInt(2u, 8);
                    datagram.WriteUInt((uint)message.Length, 32);
                    datagram.WriteBytes(message);
                }
                pendingMessages.Clear();

                foreach (var syncable in syncables)
                {
                    var syncData = new WriteOnlyDatagram();
                    syncable.WriteFullSync(syncData);
                    if (syncData.LengthInBytes > 0)
                    {
                        datagram.WriteUInt(1u, 8);
                        //datagram.WriteUInt(syncable.EntityID, 32);
                        //datagram.WriteUInt(syncable.SyncID, 8);
                        datagram.WriteUInt((uint)syncData.LengthInBytes, 32);
                        datagram.WriteBytes(syncData.BufferAsArray);
                    }
                }
                  
                netSession.broadcastCriticalDatagram(datagram.BufferAsArray);
            }
        }
        
        void IModule.EndSimulation()
        {
        }

        void IModule.AddComponents(List<Component> components)
        {
            syncables.AddRange(components.Where(c => c is ISyncable && !sim.IsLocalEntity(c.EntityID))
                .Select(c => c as ISyncable));
        }

        void IModule.RemoveEntities(List<uint> entities)
        {
            //syncables.RemoveAll(c => entities.Contains(c.EntityID));
        }

        void IModule.BindScript(MISP.Engine scriptEngine)
        {
            scriptEngine.AddFunction("get-client", "Retreive a client object.", (context, arguments) =>
                {
                    var n = MISP.AutoBind.IntArgument(arguments[0]);
                    return netSession.getPeer(n);
                }, MISP.Arguments.Arg("client"));

            scriptEngine.AddFunction("client-count", "Get the number of connected clients.", (context, arguments) =>
                {
                    return netSession.clientCount;
                });
        }

    }
}
