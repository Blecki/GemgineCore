using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Epiphany
{
    public class Circle : IShape
    {
        public TextureAlignment TextureAlignment = new TextureAlignment();
        public Vector2 Center;
        public float Radius;
        public int DrawSides = 16;

        public Circle(Vector2 Center, float Radius) 
        {
            this.Center = Center;
            this.Radius = Radius;
        }
        
        public Box2D.XNA.Shape[] GetPhysicsShapes()
        {
            var r = new Box2D.XNA.CircleShape();
            r._p = Center;
            r._radius = Radius;
            return new Box2D.XNA.Shape[1] { r };
        }

        public Mesh GetRenderMesh()
        {
            Mesh result = Geo.Gen.CreateUnitPolygon(DrawSides, new Vector3(0, Radius, 0), Vector3.UnitZ);
            Geo.Gen.ProjectTexture(result, TextureAlignment);
            return result;
        }

    }
}
