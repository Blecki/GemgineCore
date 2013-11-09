using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo.Ico
{
    public class Face
    {
        public Int16[] Verticies = new Int16[3];

        public Face(params Int16[] v)
        {
            Verticies = v;
        }
    }

    public class Edge
    {
        public Int16[] Verticies = new Int16[2];

        public Edge(Int16 a, Int16 b)
        {
            Verticies[0] = a;
            Verticies[1] = b;
        }
    }

    public class Icosahedron
    {
        internal List<Vector3> Verticies;
        internal List<Face> Faces;
        internal List<Edge> Edges;

        private static Vector3 MakeVert(float X, float Y, float Z)
        {
            return new Vector3(X, Y, Z);
        }

        public static Icosahedron Generate()
        {
            float t = (float)((1.0f + System.Math.Sqrt(5.0f)) / 2.0f);
            float s = (float)(System.Math.Sqrt(1.0f + (t * t)));

            var R = new Icosahedron();
            R.Verticies = new List<Vector3> {
                MakeVert(t, 1, 0) / s,
                MakeVert(-t, 1, 0) / s,
                MakeVert(t, -1, 0) / s,

                MakeVert(-t, -1, 0) / s,
                MakeVert(1, 0, t) / s,
                MakeVert(1, 0, -t) / s,

                MakeVert(-1, 0, t) / s,
                MakeVert(-1, 0, -t) / s,
                MakeVert(0, t, 1) / s,

                MakeVert(0, -t, 1) / s,
                MakeVert(0, t, -1) / s,
                MakeVert(0, -t, -1) / s
            };

            R.Faces = new List<Face>();
            R.Faces.Add(new Face(0, 4, 8));
            R.Faces.Add(new Face(1, 7, 10));
            R.Faces.Add(new Face(2, 11, 9));
            R.Faces.Add(new Face(7, 1, 3));

            R.Faces.Add(new Face(0, 10, 5));
            R.Faces.Add(new Face(3, 6, 9));
            R.Faces.Add(new Face(3, 9, 11));
            R.Faces.Add(new Face(8, 4, 6));

            R.Faces.Add(new Face(2, 9, 4));
            R.Faces.Add(new Face(3, 11, 7));
            R.Faces.Add(new Face(4, 0, 2));
            R.Faces.Add(new Face(9, 6, 4));

            R.Faces.Add(new Face(2, 5, 11));
            R.Faces.Add(new Face(0, 8, 10));
            R.Faces.Add(new Face(5, 2, 0));
            R.Faces.Add(new Face(10, 7, 5));

            R.Faces.Add(new Face(1, 8, 6));
            R.Faces.Add(new Face(1, 10, 8));
            R.Faces.Add(new Face(6, 3, 1));
            R.Faces.Add(new Face(11, 5, 7));

            R.FindEdges();

            return R;
        }

        private void FindEdges()
        {
            Edges = new List<Edge>();
            foreach (var Face in Faces)
            {
                var _Edges = new Edge[3];
                _Edges[0] = new Edge(Face.Verticies[0], Face.Verticies[1]);
                _Edges[1] = new Edge(Face.Verticies[1], Face.Verticies[2]);
                _Edges[2] = new Edge(Face.Verticies[2], Face.Verticies[0]);

                foreach (var Edge in _Edges)
                    if (FindEdge(Edge.Verticies[0], Edge.Verticies[1]) == -1) Edges.Add(Edge);
            }
        }

        private int FindEdge(int v0, int v1)
        {
            return Edges.FindIndex((A) =>
                {
                    return (A.Verticies[0] == v0 && A.Verticies[1] == v1) || (A.Verticies[0] == v1 && A.Verticies[1] == v0);
                });
        }

        public Icosahedron Subdivide()
        {
            var R = new Icosahedron();

            var dividedEdgeIndex = new short[Edges.Count];

            R.Verticies = new List<Vector3>();
            foreach (var Vert in Verticies) R.Verticies.Add(Vert);

            for (int E = 0; E < Edges.Count; ++E)
            {
                var NewVert = Vector3.Normalize((Verticies[Edges[E].Verticies[0]] + Verticies[Edges[E].Verticies[1]]) / 2);
                dividedEdgeIndex[E] = (short)R.Verticies.Count;
                R.Verticies.Add(NewVert);
            }

            R.Faces = new List<Face>();
            foreach (var Face in Faces)
            {
                int[] FEdges = new int[3];
                FEdges[0] = FindEdge(Face.Verticies[0], Face.Verticies[1]);
                FEdges[1] = FindEdge(Face.Verticies[1], Face.Verticies[2]);
                FEdges[2] = FindEdge(Face.Verticies[2], Face.Verticies[0]);

                R.Faces.Add(new Face(Face.Verticies[0], dividedEdgeIndex[FEdges[0]], dividedEdgeIndex[FEdges[2]]));
                R.Faces.Add(new Face(Face.Verticies[1], dividedEdgeIndex[FEdges[1]], dividedEdgeIndex[FEdges[0]]));
                R.Faces.Add(new Face(Face.Verticies[2], dividedEdgeIndex[FEdges[2]], dividedEdgeIndex[FEdges[1]]));
                R.Faces.Add(new Face(dividedEdgeIndex[FEdges[0]], dividedEdgeIndex[FEdges[1]], dividedEdgeIndex[FEdges[2]]));
            }

            R.FindEdges();

            return R;

        }

        public Icosahedron Hexagons()
        {
            var R = new Icosahedron();
            R.Verticies = new List<Vector3>();
            R.Faces = new List<Face>();



            return R;
        }

        public Mesh GenerateMesh(float Radius)
        {
            var r = new Mesh();
            r.verticies = new VertexPositionNormalTexture[Verticies.Count];

            for (int i = 0; i < Verticies.Count; ++i)
                r.verticies[i] = new VertexPositionNormalTexture(Verticies[i] * Radius, Vector3.Normalize(Verticies[i]), Vector2.Zero);

            r.indicies = new short[Faces.Count * 3];
            int index = 0;
            foreach (var Face in Faces)
            {
                r.indicies[index] = Face.Verticies[0];
                r.indicies[index + 1] = Face.Verticies[1];
                r.indicies[index + 2] = Face.Verticies[2];
                index += 3;
            }

            return r;
        }

    }
}
