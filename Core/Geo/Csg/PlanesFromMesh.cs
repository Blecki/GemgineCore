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
        public static Mesh ConvertToMesh(RealtimeCSG.CSGMesh csg)
        {
            var result = new Mesh();
            result.verticies = new VertexPositionNormalTexture[csg.Vertices.Count];
            for (int i = 0; i < csg.Vertices.Count; ++i)
                result.verticies[i].Position = new Vector3(csg.Vertices[i].X, csg.Vertices[i].Y, csg.Vertices[i].Z);

            var indicies = new List<short>();
            foreach (var polygon in csg.Polygons)
            {
                if (polygon.Visible && polygon.FirstIndex >= 0)
                {
                    var indexList = new List<short>();
                    var start = csg.Edges[polygon.FirstIndex];
                    indexList.Add(start.VertexIndex);
                    var current = csg.Edges[start.NextIndex];
                    while (current != start)
                    {
                        indexList.Add(current.VertexIndex);
                        current = csg.Edges[current.NextIndex];
                    }

                    for (var index = 0; index < indexList.Count - 2; ++index)
                    {
                       
                        indicies.Add(indexList[0]);

                        if (polygon.Category == RealtimeCSG.PolygonCategory.Aligned)
                        {
                            indicies.Add(indexList[index + 2]);
                            indicies.Add(indexList[index + 1]);
                        }
                        else
                        {
                            indicies.Add(indexList[index + 1]);
                            indicies.Add(indexList[index + 2]);
                        }
                    }
                }
            }

            result.indicies = indicies.ToArray();

            return result;
        }
    }
}