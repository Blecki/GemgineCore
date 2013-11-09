using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        //Merge identicle verticies in mesh
        public static Mesh WeldCopy(Mesh m)
        {
            var resultVerticies = new List<VertexPositionNormalTexture>();
            var indexMap = new Dictionary<short, short>();

            for (short i = 0; i < m.verticies.Length; i += 1)
            {
                var pos = m.verticies[i].Position;
                bool matchFound = false;
                for (int x = 0; x < resultVerticies.Count; ++x)
                {
                    if (Gem.Math.Utility.AlmostZero((resultVerticies[x].Position - pos).LengthSquared()))
                    {
                        indexMap.Add(i, (short)x);
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    indexMap.Add(i, (short)resultVerticies.Count);
                    resultVerticies.Add(m.verticies[i]);
                }
            }

            var result = new Mesh();
            result.verticies = resultVerticies.ToArray();
            result.indicies = new short[m.indicies.Length];

            for (int i = 0; i < result.indicies.Length; ++i)
                result.indicies[i] = indexMap[m.indicies[i]];

            return result;
        }
    }
}