using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Epiphany
{
    public class Polygon : List<Vector2>, IShape
    {
        public TextureAlignment TextureAlignment = new TextureAlignment();

        public Polygon(IEnumerable<Vector2> source) { this.AddRange(source); }
        public Polygon() { }

        public Box2D.XNA.Shape[] GetPhysicsShapes()
        {
            if (Decomposition.IsConvex(this))
            {
                var pShape = new Box2D.XNA.PolygonShape();
                pShape.Set(this.ToArray(), this.Count);
                return new Box2D.XNA.Shape[1] { pShape };
            }

            List<Polygon> decomposedPolygons;
            Decomposition.DecomposeConvex(this, out decomposedPolygons, 8);
            var r = new Box2D.XNA.Shape[decomposedPolygons.Count];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = new Box2D.XNA.PolygonShape();
                (r[i] as Box2D.XNA.PolygonShape).Set(decomposedPolygons[i].ToArray(), decomposedPolygons[i].Count);
            }
            return r;
        }

        public Mesh GetRenderMesh()
        {
            Mesh result = null;
            if (Decomposition.IsConvex(this))
                result = Geo.Gen.CreatePolygonMeshFrom2D(this);
            else
            {
                List<Polygon> decomposedPolygons;
                Decomposition.DecomposeConvex(this, out decomposedPolygons, 32);
                List<Mesh> meshes = new List<Mesh>();
                foreach (var polygon in decomposedPolygons)
                    meshes.Add(Geo.Gen.CreatePolygonMeshFrom2D(polygon));
                result = Geo.Gen.Merge(meshes.ToArray());
            }
            Geo.Gen.ProjectTexture(result, TextureAlignment);
            return result;
        }

        public static Polygon CreateQuad(float w, float h)
        {
            var result = new Polygon();
            result.Add(new Vector2(-w / 2, -h / 2));
            result.Add(new Vector2(w / 2, -h / 2));
            result.Add(new Vector2(w / 2, h / 2));
            result.Add(new Vector2(-w / 2, h / 2));
            return result;
        }

        public static Polygon CreateUnitPolygon(int sides, float radius)
        {
            var result = new Polygon();
            for (int i = 0; i < sides; ++i)
            {
                var matrix = Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians((360.0f / sides) * i));
                result.Add(Vector2.Transform(new Vector2(0, radius), matrix));
            }
            return result;
        }

    }
}
