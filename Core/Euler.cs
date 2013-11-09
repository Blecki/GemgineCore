using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class Euler : Network.ISyncable
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Vector3 Orientation = Vector3.Zero;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Scale)
                    * Matrix.CreateFromYawPitchRoll(Orientation.X, Orientation.Y, Orientation.Z)
                    * Matrix.CreateTranslation(Position);
            }
        }

        void Network.ISyncable.WriteFullSync(Network.WriteOnlyDatagram datagram)
        {
            datagram.WriteBytes(BitConverter.GetBytes(Position.X));
            datagram.WriteBytes(BitConverter.GetBytes(Position.Y));
            datagram.WriteBytes(BitConverter.GetBytes(Position.Z));
            datagram.WriteBytes(BitConverter.GetBytes(Scale.X));
            datagram.WriteBytes(BitConverter.GetBytes(Scale.Y));
            datagram.WriteBytes(BitConverter.GetBytes(Scale.Z));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.X));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.Y));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.Z));
        }

        void Network.ISyncable.ReadFullSync(Network.ReadOnlyDatagram datagram)
        {
            var data = new byte[sizeof(Single) * 9];
            datagram.ReadBytes(data, sizeof(Single) * 9);

            Position = new Vector3(BitConverter.ToSingle(data, sizeof(Single) * 0),
                BitConverter.ToSingle(data, sizeof(Single) * 1),
                BitConverter.ToSingle(data, sizeof(Single) * 2));

            Scale = new Vector3(BitConverter.ToSingle(data, sizeof(Single) * 3),
                BitConverter.ToSingle(data, sizeof(Single) * 4),
                BitConverter.ToSingle(data, sizeof(Single) * 5));

            Orientation = new Vector3(BitConverter.ToSingle(data, sizeof(Single) * 6),
                BitConverter.ToSingle(data, sizeof(Single) * 7),
                BitConverter.ToSingle(data, sizeof(Single) * 8));
        }
    }
}
