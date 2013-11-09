using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Epiphany
{
    public class Edge : List<Vector2>, IShape
    {
        public TextureAlignment TextureAlignment = new TextureAlignment();
        public float DrawWidth = 0.1f;
        public bool Loop = false;

        public Edge(IEnumerable<Vector2> source) { this.AddRange(source); }
        public Edge() { }

        //TODO: Loop
        public Box2D.XNA.Shape[] GetPhysicsShapes()
        {
            System.Diagnostics.Debug.Assert(Count >= 2);

            var r = new Box2D.XNA.Shape[Count - 1];
            for (int i = 1; i < Count; ++i)
            {
                var edge = new Box2D.XNA.EdgeShape();
                edge.Set(this[i - 1], this[i]);
                if (i > 2)
                {
                    edge._hasVertex0 = true;
                    edge._vertex0 = this[i - 2];
                }
                if (i < (Count - 1))
                {
                    edge._hasVertex3 = true;
                    edge._vertex3 = this[i + 1];
                }
                r[i - 1] = edge;
            }

            return r;
        }

        private Mesh CreateSegment(Vector2 A, Vector2 B)
        {
            var T = B - A;
            var N = new Vector2(T.Y, -T.X);
            N.Normalize();
            N *= (DrawWidth / 2);

            var part = Geo.Gen.CreateQuad();
            part.verticies[0].Position = new Vector3(A + N, 0);
            part.verticies[1].Position = new Vector3(B + N, 0);
            part.verticies[2].Position = new Vector3(B - N, 0);
            part.verticies[3].Position = new Vector3(A - N, 0);

            return part;
        }

        public Mesh GetRenderMesh()
        {
            System.Diagnostics.Debug.Assert(Count >= 2);

            var parts = new List<Mesh>();

            for (int i = 1; i < Count; ++i)
                parts.Add(CreateSegment(this[i - 1], this[i]));

            if (Loop) parts.Add(CreateSegment(this[Count - 1], this[0]));

            var result = Geo.Gen.Merge(parts.ToArray());
            Geo.Gen.ProjectTexture(result, TextureAlignment);
            return result;
        }

    }
}
