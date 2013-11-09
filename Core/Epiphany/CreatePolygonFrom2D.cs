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
        public static Mesh CreatePolygonMeshFrom2D(List<Vector2> points)
        {
            var result = new Mesh();
            result.verticies = new VertexPositionNormalTexture[points.Count];
            for (int i = 0; i < points.Count; ++i)
                result.verticies[i] = new VertexPositionNormalTexture(
                    new Vector3(points[i], 0), Vector3.UnitZ, Vector2.Zero);

            result.indicies = new short[(points.Count - 2) * 3];
            short index = 0;
            for (index = 0; index < points.Count - 2; ++index)
            {
                result.indicies[(index * 3)] = 0;
                result.indicies[(index * 3) + 1] = (short)(index + 1);
                result.indicies[(index * 3) + 2] = (short)(index + 2);
            }
           
            return result;
        }
    }
}