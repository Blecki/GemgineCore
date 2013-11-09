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
        /// <summary>
        /// Align the object with the spline. Transforms object so that its origin is on the spline.
        /// </summary>
        /// <returns></returns>
        public static void AlignToSpline(
            Mesh mesh,
            Vector3 axis, 
            Vector3 splineRotationAxis,
            Gem.Math.Spline spline,
            float distance)
        {
            Morph(mesh, (v) =>
                {
                    var splinePoint = spline.Point(distance);
                    var splineTangent = spline.Tangent(distance);
                    var m = Matrix.CreateFromAxisAngle(Vector3.Normalize(splineRotationAxis),
                        Gem.Math.Vector.AngleBetweenVectors(axis, splineTangent));
                    return splinePoint + Vector3.Transform(v, m);
                });
        }

        public static Mesh AlignToSplineCopy(
            Mesh mesh,
            Vector3 axis,
            Vector3 splineRotationAxis,
            Gem.Math.Spline spline,
            float distance)
        {
            var result = Copy(mesh);
            AlignToSpline(result, axis, splineRotationAxis, spline, distance);
            return result;
        }
    }
}