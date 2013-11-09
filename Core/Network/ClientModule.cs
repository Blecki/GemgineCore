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
    public class ClientModule : IModule
    {
        Network.ClientSession netSession;
        private Simulation simulation;
        private Dictionary<UInt32, List<ISyncable>> syncables = new Dictionary<uint, List<ISyncable>>();
        
        public ClientModule(System.Net.IPAddress host, int port, ClientGame parentGame)
        {
                netSession = new Network.ClientSession(0, new System.Net.IPEndPoint(host, port), 
                    parentGame.Main.ScriptConsole.Write);

                netSession.onDatagramReceived += (data) =>
                {
                    var gram = new Network.ReadOnlyDatagram(data);
                    while (gram.More)
                    {
                        uint messageCode = 0;
                        gram.ReadUInt(out messageCode, 8);

                        switch (messageCode)
                        {
                            case 0:
                                if (simulation != null) throw new InvalidProgramException();
                                {
                                    String episodeName;
                                    uint version;
                                    gram.ReadString(out episodeName);
                                    gram.ReadUInt(out version, 32);
                                    parentGame.StartSimulation(episodeName, version);
                                }
                                break;
                            case 1:
                                {
                                    UInt32 entityID;
                                    UInt32 syncID;
                                    UInt32 dataLength;
                                    gram.ReadUInt(out entityID, 32);
                                    gram.ReadUInt(out syncID, 8);
                                    gram.ReadUInt(out dataLength, 32);
                                    var bytes = new byte[dataLength];
                                    gram.ReadBytes(bytes, dataLength);

                                    if (syncables.ContainsKey(entityID))
                                        foreach (var syncable in syncables[entityID])
                                            //if (syncable.SyncID == syncID)
                                                syncable.ReadFullSync(new ReadOnlyDatagram(bytes));
                                }
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
                                    simulation.EnqueueEvent(messageID, messageData);
                                }
                                break;
                        }
                    }
                };

                parentGame.Main.Write("Connected to server " + host + " on port " + port + "\n");
            
        }

        void IModule.BeginSimulation(Simulation sim)
        {
            this.simulation = sim;
            simulation.sendMessageHandler += (bytes) => {
                var datagram = new WriteOnlyDatagram();
                datagram.WriteUInt(2u, 8);
                datagram.WriteUInt((uint)bytes.Length, 32);
                datagram.WriteBytes(bytes);
                netSession.sendCriticalDatagram(datagram.BufferAsArray); 
            };
        }

        void IModule.Update(float elapsedSeconds)
        {
            netSession.update();
        }

        void IModule.EndSimulation()
        {

        }

        void IModule.AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
                if (!(component is ISyncable) || simulation.IsLocalEntity(component.EntityID)) continue;
                if (!syncables.ContainsKey(component.EntityID))
                    syncables.Upsert(component.EntityID, new List<ISyncable>());
                syncables[component.EntityID].Add(component as ISyncable);
            }
        }

        void IModule.RemoveEntities(List<uint> entities)
        {
            foreach (var id in entities)
                if (syncables.ContainsKey(id)) syncables.Remove(id);
        }

        void IModule.BindScript(MISP.Engine scriptEngine)
        {

        }

    }
}
