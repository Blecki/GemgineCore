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
        internal class Edge
        {
            public short S;
            public short E;
            public short C;

            internal Edge(short S, short E)
            {

                this.S = S;
                this.E = E;
            }
        }

        internal static bool CompareEdges(Edge A, Edge B)
        {
            if (A.S == B.S && A.E == B.E) return true;
            if (A.E == B.S && A.S == B.E) return true;
            return false;
        }

        public Mesh SubdivideCopy(Mesh mesh)
        {
            var edges = new List<Edge>();

            var newIndicies = new short[mesh.indicies.Length * 4];
            var vertsAdded = 0;
            var triEdges = new Edge[3];
            for (int tri = 0; tri < mesh.indicies.Length / 3; ++tri)
            {
                triEdges[0] = new Edge(mesh.indicies[tri * 3], mesh.indicies[tri * 3 + 1]);
                triEdges[1] = new Edge(mesh.indicies[tri * 3 + 1], mesh.indicies[tri * 3 + 2]);
                triEdges[2] = new Edge(mesh.indicies[tri * 3 + 2], mesh.indicies[tri * 3]);

                foreach (var edge in triEdges)
                {
                    var existingEdge = edges.FirstOrDefault((e) => { return CompareEdges(e, edge); });
                    if (existingEdge != null)
                    {
                        edge.C = existingEdge.C;
                    }
                    else
                    {
                        edges.Add(edge);
                        edge.C = (short)(mesh.verticies.Length + vertsAdded);
                        vertsAdded += 1;
                    }
                }

                /*
                 *          E S
                 *         /   \
                 *        /     \
                 *    2  C ----  C    0
                 *      / \     / \
                 *     /   \   /   \
                 *    S     \ /     E
                 *      E----C----S
                 *           1
                 */

                CopyIndicies(newIndicies, tri * 3 * 4, new short[] {
                    triEdges[0].S, triEdges[0].C, triEdges[2].C,
                    triEdges[0].C, triEdges[0].E, triEdges[1].C,
                    triEdges[1].C, triEdges[1].E, triEdges[2].C,
                    triEdges[0].C, triEdges[1].C, triEdges[2].C
                });
            }

            var newVerticies = new VertexPositionNormalTexture[mesh.verticies.Length + edges.Count];
            for (int i = 0; i < mesh.verticies.Length; ++i) newVerticies[i] = mesh.verticies[i];

            foreach (var edge in edges)
            {
                newVerticies[edge.C] = mesh.verticies[edge.S];
                newVerticies[edge.C].Position = (mesh.verticies[edge.S].Position + mesh.verticies[edge.E].Position) / 2;
            }

            var r = new Mesh();
            r.verticies = newVerticies;
            r.indicies = newIndicies;
            return r;
        }
    }
}