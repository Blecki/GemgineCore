using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public class PlanarProjection : TextureProjection
    {
        public RealtimeCSG.Plane plane;
        public Vector3 upHint;
        public float Scale = 1.0f;

        public PlanarProjection(RealtimeCSG.Plane plane, Vector3 upHint, float Scale)
        {
            this.plane = plane;
            this.upHint = upHint;
            this.Scale = Scale;
        }

        public override void Project(Mesh onto, Matrix objectWorldTransformation)
        {
            var tPlane = Csg.TransformCopy(plane, objectWorldTransformation);
            var tHint = Vector3.Transform(upHint, objectWorldTransformation);
            var perpendicularAxis = new Vector3(tPlane.A, tPlane.B, tPlane.C);
            perpendicularAxis.Normalize();
            var cross = Vector3.Cross(perpendicularAxis, tHint);

            Gen.ProjectTexture(onto, perpendicularAxis * plane.D,
                cross * Scale,
                Vector3.Cross(perpendicularAxis, cross) * Scale);
        }
    }
}
