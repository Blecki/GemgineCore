using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Csg
    {
        public static RealtimeCSG.Plane[] CreatePrism(int sides, Vector3 radial, Vector3 axis, float height)
        {
            var result = new List<RealtimeCSG.Plane>();
            result.Add(new RealtimeCSG.Plane(AsCsgVector(axis), height / 2));
            result.Add(new RealtimeCSG.Plane(AsCsgVector(-axis), height / 2));

            var radialLength = radial.Length();

            for (int i = 0; i < sides; ++i)
            {
                var sideNormal = Vector3.Transform(radial, Matrix.CreateFromAxisAngle(axis, i * (Gem.Math.Angle.PI2 / sides)));
                result.Add(new RealtimeCSG.Plane(AsCsgVector(Vector3.Normalize(sideNormal)), radialLength));
            }

            return result.ToArray();
        }

        private static RealtimeCSG.Vector3 AsCsgVector(Vector3 v)
        {
            return new RealtimeCSG.Vector3(v.X, v.Y, v.Z);
        }
    }
}