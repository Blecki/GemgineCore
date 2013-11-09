using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public class EMFace
    {
        public List<EMEdge> edges = new List<EMEdge>();
        //public Int16[] Verticies = new Int16[3];
        //public EMEdge[] Edges = new EMEdge[3];
        //public EMFace[] Neighbors = new EMFace[3];
        public Vector3 Centroid;

        public EMFace(params Int16[] v)
        {
            for (int i = 0; i < v.Length; ++i)
            {
                var o = i - 1;
                if (o == -1) o = v.Length - 1;
                edges.Add(new EMEdge(v[o], v[i]));
            }
            //Verticies = v;
            
            //Edges[0] = new EMEdge(v[0],v[1]);
            //Edges[1] = new EMEdge(v[1],v[2]);
            //Edges[2] = new EMEdge(v[2],v[0]);

            foreach (var e in edges)
            {
                e.ParentFace = this;
                e.Neighbors[0] = this;
            }

            //for (int i = 0; i < 3; ++i) edges[i].ParentFace = this;

            Centroid = Vector3.Zero;
        }

        internal void SetRecipricalNeighbor(EMEdge edge, EMFace neighbor)
        {
            foreach (var e in edges)
                if (Object.ReferenceEquals(e, edge)) e.Neighbors[1] = neighbor;
            //for (int i = 0; i < 3; ++i)
            //    if (Object.ReferenceEquals(Edges[i],edge)) Neighbors[i] = neighbor;
        }
    }

    public class EMEdge
    {
        public Int16[] Verticies = new Int16[2];
        public EMFace ParentFace = null;
        public EMFace[] Neighbors = new EMFace[2];

        public EMEdge(Int16 a, Int16 b)
        {
            Verticies[0] = a;
            Verticies[1] = b;
        }
    }

    public partial class EdgeMesh
    {
        public List<Vector3> Verticies;
        public List<EMFace> Faces;
        public List<EMEdge> Edges;
                
        private int FindEdge(int v0, int v1)
        {
            return Edges.FindIndex((A) =>
                {
                    return (A.Verticies[0] == v0 && A.Verticies[1] == v1) || (A.Verticies[0] == v1 && A.Verticies[1] == v0);
                });
        }

        public EdgeMesh()
        {
            this.Verticies = new List<Vector3>();
            this.Faces = new List<EMFace>();
            this.Edges = new List<EMEdge>();
        }

        public EdgeMesh(Mesh m)
        {
            this.Verticies = new List<Vector3>();
            this.Faces = new List<EMFace>();
            this.Edges = new List<EMEdge>();

            foreach (var v in m.verticies) this.Verticies.Add(v.Position);

            for (int i = 0; i < m.indicies.Length; i += 3)
            {
                var face = new EMFace(m.indicies[i + 0], m.indicies[i + 1], m.indicies[i + 2]);

                foreach (var edge in face.edges)
                    face.Centroid += this.Verticies[edge.Verticies[0]];
                face.Centroid /= face.edges.Count;

                this.Faces.Add(face);
                for (int e = 0; e < face.edges.Count; ++e)
                {
                    var foundEdge = FindEdge(face.edges[e].Verticies[0], face.edges[e].Verticies[1]);
                    if (foundEdge != -1)
                    {
                        face.edges[e] = this.Edges[foundEdge];
                        
                        face.edges[e].ParentFace.SetRecipricalNeighbor(face.edges[e], face);
                    }
                    else
                        this.Edges.Add(face.edges[e]);
                }
            }
        }

        public EMFace FaceAt(Vector3 p)
        {
            foreach (var face in Faces)
                if (IsPointOnFace(p, face)) return face;
            return null;
        }

        public Vector3 FindEdgeCenter(EMEdge e)
        {
            return (Verticies[e.Verticies[0]] + Verticies[e.Verticies[1]]) / 2.0f;
        }
    }
}
