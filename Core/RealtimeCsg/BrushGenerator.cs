using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeCSG
{
    public class BrushGenerator
    {
        public virtual Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation) { throw new NotImplementedException(); }
    }

    public class StaticBrushGenerator : BrushGenerator
    {
        public Plane[] Planes;
        public StaticBrushGenerator(Plane[] planes) { this.Planes = planes; }
        public override Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation)
        {
            return Gem.Geo.Csg.TransformCopy(Planes, transformation);
        }
    }

    public class CuboidGenerator : BrushGenerator
    {
        public override Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation)
        {
            var planes = Gem.Geo.Csg.CreateCube();
            return Gem.Geo.Csg.TransformCopy(planes, transformation);
        }
    }

    public class PrismGenerator : BrushGenerator
    {
        public int Sides = 3;
        public float Height = 1.0f;

        public PrismGenerator(int Sides, float Height)
        {
            this.Sides = Sides;
            this.Height = Height;
        }

        public override Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation)
        {
            var planes = Gem.Geo.Csg.CreatePrism(Sides, Microsoft.Xna.Framework.Vector3.UnitY, Microsoft.Xna.Framework.Vector3.UnitZ, Height);
            return Gem.Geo.Csg.TransformCopy(planes, transformation);
        }
    }

    public class PyramidGenerator : BrushGenerator
    {
        public int Sides = 3;
        public float Height = 1.0f;

        public PyramidGenerator(int Sides, float Height)
        {
            this.Sides = Sides;
            this.Height = Height;
        }

        public override Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation)
        {
            var planes = Gem.Geo.Csg.CreatePyramid(Sides, Microsoft.Xna.Framework.Vector3.UnitY, Microsoft.Xna.Framework.Vector3.UnitZ, Height);
            return Gem.Geo.Csg.TransformCopy(planes, transformation);
        }
    }

    public class IcosahedronGenerator : BrushGenerator
    {
        public float Radius = 1.0f;

        public IcosahedronGenerator(float Radius = 1.0f)
        {
            this.Radius = Radius;
        }

        public override Plane[] GetPlanes(Microsoft.Xna.Framework.Matrix transformation)
        {
            var planes = Gem.Geo.Csg.PlanesFromMesh(Gem.Geo.Ico.Icosahedron.Generate().GenerateMesh(Radius));
            return Gem.Geo.Csg.TransformCopy(planes, transformation);
        }
    }
}
