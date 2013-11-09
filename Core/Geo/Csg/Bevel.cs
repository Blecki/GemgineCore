using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Csg
    {
        private static bool IsValid(Vector3 v)
        {
            return (!float.IsNaN(v.X) && !float.IsInfinity(v.X) &&
                    !float.IsNaN(v.Y) && !float.IsInfinity(v.Y) &&
                    !float.IsNaN(v.Z) && !float.IsInfinity(v.Z));
        }

        public static RealtimeCSG.Plane[] BevelCopy(RealtimeCSG.Plane[] planes, float bevelSize)
        {
            var r = new List<RealtimeCSG.Plane>();
            r.AddRange(planes);

            for (int a = 0; a < planes.Length; ++a)
                for (int b = a + 1; b < planes.Length; ++b)
                {
                    var va = new Vector3(planes[a].A, planes[a].B, planes[a].C);
                    var vb = new Vector3(planes[b].A, planes[b].B, planes[b].C);

                    var nv = va + vb;
                    nv.Normalize();
                    if (!IsValid(nv)) continue;

                    va.Normalize();
                    vb.Normalize();
                    float DotProduct = Vector3.Dot(va, vb);
                    DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);

                    var angle = (float)System.Math.Acos(DotProduct);
                    var cos = (float)System.Math.Cos(angle / 2.0f);
                    var nd = planes[a].D / cos;
                    nd -= bevelSize;

                    if (nd < 0) continue;
                    r.Add(new RealtimeCSG.Plane(nv.X, nv.Y, nv.Z, nd));
                }

            return r.ToArray();
        }
    }
}
