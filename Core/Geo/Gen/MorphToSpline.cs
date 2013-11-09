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
        /// Morph the object onto a spline. Expects the object's origin to already be aligned with the spline.
        /// </summary>
        /// <returns></returns>
        public static void MorphToSpline(
            Mesh mesh,
            Vector3 axis, 
            Gem.Math.Spline spline)
        {
            Morph(mesh, (v) =>
                {
                    var distance = Gem.Math.Vector.ProjectAOntoB(v, axis).Length() / axis.Length();
                    var splinePoint = spline.Point(distance);
                    var splineTangent = spline.Tangent(distance);
                    var rel = (axis * distance) - v;
                    var m = Matrix.CreateFromAxisAngle(Vector3.Normalize(spline.RotationAxis),//splineRotationAxis),
                        Gem.Math.Vector.AngleBetweenVectors(axis, splineTangent));


                    return splinePoint + Vector3.Transform(rel, m);
                });
        }

        public static Mesh MorphToSplineCopy(
            Mesh mesh,
            Vector3 axis,
            Gem.Math.Spline spline)
        {
            var result = Copy(mesh);
            MorphToSpline(result, axis, spline);
            return result;
        }
    }
}