using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Network
{
    public static class ScriptMessage
    {
        public enum ScriptTypes
        {
            Int32 = 0,
            UInt32 = 1,
            String = 2,
            Single = 3,
            List = 4,
            Bool = 5,
        }

        private static Dictionary<System.Type, ScriptTypes> typeCodes = null;

        private static void Initialize()
        {
            if (typeCodes != null) return;
            typeCodes = new Dictionary<Type,ScriptTypes>();
            typeCodes.Upsert(typeof(Int32), ScriptTypes.Int32);
            typeCodes.Upsert(typeof(UInt32), ScriptTypes.UInt32);
            typeCodes.Upsert(typeof(String), ScriptTypes.String);
            typeCodes.Upsert(typeof(Single), ScriptTypes.Single);
            typeCodes.Upsert(typeof(Common.ObjectList), ScriptTypes.List);
        }

        public static byte[] EncodeMessage(String ID, Common.ObjectList Data)
        {
            Initialize();
            var datagram = new WriteOnlyDatagram();
            datagram.WriteString(ID);
            EncodeList(Data, datagram);
            return datagram.BufferAsArray;
        }

        public static void DecodeMessage(byte[] bytes, out String ID, out Common.ObjectList Data)
        {
            Initialize();
            var datagram = new ReadOnlyDatagram(bytes);
            datagram.ReadString(out ID);
            Data = DecodeList(datagram);
        }

        private static void EncodeList(Common.ObjectList data, WriteOnlyDatagram datagram)
        {
            datagram.WriteUInt((uint)data.Count, 8);
            foreach (var item in data)
            {
                if (item == null)
                {
                    datagram.WriteUInt((uint)ScriptTypes.Bool, 8);
                    datagram.WriteUInt(0u, 8);
                    continue;
                }

                var type = item.GetType();
                if (typeCodes.ContainsKey(type))
                {
                    var typeCode = typeCodes[type];
                    datagram.WriteUInt((uint)typeCode, 8);
                    switch (typeCode)
                    {
                        case ScriptTypes.List:
                            EncodeList(item as Common.ObjectList, datagram);
                            break;
                        case ScriptTypes.String:
                            datagram.WriteString(item as String);
                            break;
                        case ScriptTypes.Int32:
                            datagram.WriteBytes(BitConverter.GetBytes((item as int?).Value));
                            break;
                        case ScriptTypes.UInt32:
                            datagram.WriteBytes(BitConverter.GetBytes((item as uint?).Value));
                            break;
                        case ScriptTypes.Single:
                            datagram.WriteBytes(BitConverter.GetBytes((item as float?).Value));
                            break;
                        case ScriptTypes.Bool:
                            datagram.WriteUInt((item as bool?).Value ? 1u : 0u, 8);
                            break;
                        default:
                            throw new InvalidProgramException("Error encoding message", null);
                    }
                }
                else
                {
                    throw new InvalidProgramException("Type " + type.Name + " cannot be serialized to a network message.", null);
                }
            }
        }

        private static Common.ObjectList DecodeList(ReadOnlyDatagram datagram)
        {
            var r = new Common.ObjectList();
            uint count = 0;
            datagram.ReadUInt(out count, 8);
            byte[] temp = new byte[4];
            for (int i = 0; i < count; ++i)
            {
                uint typeCode = 0;
                datagram.ReadUInt(out typeCode, 8);
                switch ((ScriptTypes)typeCode)
                {
                    case ScriptTypes.List:
                        r.Add(DecodeList(datagram));
                        break;
                    case ScriptTypes.String:
                        {
                            String s;
                            datagram.ReadString(out s);
                            r.Add(s);
                        }
                        break;
                    case ScriptTypes.Int32:
                        datagram.ReadBytes(temp, 4);
                        r.Add(BitConverter.ToInt32(temp, 0));
                        break;
                    case ScriptTypes.UInt32:
                        datagram.ReadBytes(temp, 4);
                        r.Add(BitConverter.ToUInt32(temp, 0));
                        break;
                    case ScriptTypes.Single:
                        datagram.ReadBytes(temp, 4);
                        r.Add(BitConverter.ToSingle(temp, 0));
                        break;
                    case ScriptTypes.Bool:
                        {
                            uint b = 0;
                            datagram.ReadUInt(out b, 8);
                            if (b == 0) r.Add(null);
                            else r.Add(true);
                        }
                        break;
                    default:
                        throw new InvalidProgramException("Error decoding message", null);
                }
            }
            return r;
        }
    }
}
