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
        public static RealtimeCSG.Plane[] CreatePyramid(int sides, Vector3 radial, Vector3 axis, float height)
        {
            var result = new List<RealtimeCSG.Plane>();
            result.Add(new RealtimeCSG.Plane(AsCsgVector(axis), 0));

            var radialLength = radial.Length();

            var points = new Vector3[sides];
            for (int i = 0; i < sides; ++i)
                points[i] = Vector3.Transform(radial, Matrix.CreateFromAxisAngle(axis, i * (Gem.Math.Angle.PI2 / sides)));
            var top = axis * height;

            for (int i = 0; i < sides; ++i)
            {
                var xnaPlane = new Plane(points[i == sides - 1 ? 0 : i + 1], points[i], top);
                result.Add(new RealtimeCSG.Plane(xnaPlane.Normal.X, xnaPlane.Normal.Y, xnaPlane.Normal.Z, xnaPlane.D));
            }

            return result.ToArray();
        }
    }
}