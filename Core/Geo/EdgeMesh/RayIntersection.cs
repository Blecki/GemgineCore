using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class EdgeMesh
    {
        public bool IsPointOnFace(Vector3 p, EMFace face)
        {
            var accumulatedAngle = 0.0f;

            foreach (var edge in face.edges)
            {
                var v0 = Verticies[edge.Verticies[0]] - p;
                var v1 = Verticies[edge.Verticies[1]] - p;
                var angle = AngleBetweenVectors(v0, v1);
                if (angle < 0) angle *= -1;
                accumulatedAngle += angle;
            }

            if (System.Math.Abs((System.Math.PI * 2) - accumulatedAngle) < 0.01) return true;
            return false;
            //return Gem.Math.Utility.NearlyEqual(accumulatedAngle, Gem.Math.Angle.PI2);
        }

        private Plane ConstructPlane(EMFace face)
        {
            Plane? r = null;

            //Choose the first two edges of the face that are not colinear, and create a plane from them.
            VisitEdgePairsInSequence(face, (A, B) =>
                {
                    if (EdgesAreColinear(A, B)) return VEPISReturnCode.Advance;

                    var points = ExtractPointSequence(A, B);
                    r = new Plane(Verticies[points[0]], Verticies[points[1]], Verticies[points[2]]);
                    return VEPISReturnCode.Abort;
                });

            //If we made it here, there are no non-colinear edge pairs.
            if (!r.HasValue) throw new InvalidOperationException("Attempt to construct plane from degenerate face");
            return r.Value;
        }

        public class RayIntersectionResult
        {
            public EMFace face;
            public float distance;
        }

        public RayIntersectionResult RayIntersection(Ray ray)
        {
            var closestIntersection = new RayIntersectionResult{ distance = float.PositiveInfinity, face = null };

            foreach (var face in Faces)
            {
                var plane = ConstructPlane(face);
                var intersectionDistance = ray.Intersects(plane);
                if (intersectionDistance.HasValue && intersectionDistance.Value < closestIntersection.distance)
                {
                    var intersectionPoint = ray.Position + (ray.Direction * intersectionDistance.Value);
                    if (IsPointOnFace(intersectionPoint, face))
                    {
                        closestIntersection.distance = intersectionDistance.Value;
                        closestIntersection.face = face;
                    }
                }
            }

            if (closestIntersection.face == null) return null;
            return closestIntersection;
        }
    }
}
