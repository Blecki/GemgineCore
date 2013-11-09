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
        public static RealtimeCSG.Plane[] CreateCube()
        {
            var result = new List<RealtimeCSG.Plane>();
            result.Add(new RealtimeCSG.Plane(0, 0, -1, 0.5f));
            result.Add(new RealtimeCSG.Plane(0, 0, 1, 0.5f));
            result.Add(new RealtimeCSG.Plane(-1, 0, 0, 0.5f));
            result.Add(new RealtimeCSG.Plane(1, 0, 0, 0.5f));
            result.Add(new RealtimeCSG.Plane(0, -1, 0, 0.5f));
            result.Add(new RealtimeCSG.Plane(0, 1, 0, 0.5f));
            return result.ToArray();
        }
    }
}