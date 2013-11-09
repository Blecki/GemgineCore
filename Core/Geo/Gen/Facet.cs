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
        //Explode mesh into unique triangles for facetted look.
        public static Mesh FacetCopy(Mesh m)
        {
            var result = new Mesh();
            result.verticies = new VertexPositionNormalTexture[m.indicies.Length];
            result.indicies = new short[m.indicies.Length];

            for (short i = 0; i < m.indicies.Length; ++i)
            {
                result.verticies[i] = m.verticies[m.indicies[i]];
                result.indicies[i] = i;
            }

            for (short i = 0; i < result.verticies.Length; i += 3)
            {
                var normal = Gen.CalculateNormal(result, i, i + 1, i + 2);
                for (int j = 0; j < 3; ++j)
                    result.verticies[i + j].Normal = normal;
            }
            return result;
        }
    }
}