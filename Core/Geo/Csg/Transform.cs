using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Csg
    {
        public static RealtimeCSG.Plane TransformCopy(RealtimeCSG.Plane plane, Matrix by)
        {
            var lv = new Vector4(plane.A, plane.B, plane.C, plane.D);
            lv = Vector4.Transform(lv, by);
            var lv3 = new Vector3(lv.X, lv.Y, lv.Z);
            lv.W *= lv3.Length();
            lv3.Normalize();
            lv.X = lv3.X;
            lv.Y = lv3.Y;
            lv.Z = lv3.Z;
            return new RealtimeCSG.Plane(lv.X, lv.Y, lv.Z, lv.W);
        }

        public static RealtimeCSG.Plane[] TransformCopy(RealtimeCSG.Plane[] planes, Matrix by)
        {
            var r = new RealtimeCSG.Plane[planes.Length];
            for (int i = 0; i < planes.Length; ++i)
                r[i] = TransformCopy(planes[i], by);
            return r;
        }
    }
}
