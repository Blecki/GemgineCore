using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
    public class Bezier : Spline
    {
        public Vector3 A, B, C;
        public Bezier(Vector3 A, Vector3 B, Vector3 C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public Vector3 RotationAxis { get { return Vector3.Normalize(Vector3.Cross(B - A, C - B)); } }

        public Vector3 Point(float distance)
        {
            return Gem.Math.Bezier.Point(A, B, C, distance);
        }

        public Vector3 Tangent(float distance)
        {
            return Gem.Math.Bezier.Tangent(A, B, C, distance);
        }

        public static float sq(float f) { return f * f; }
        public static float cube(float f) { return f * f * f; }

        public static Vector2 Point(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t)
        {
            return cube(1.0f - t) * P0
                + 3.0f * sq(1.0f - t) * t * P1
                + 3.0f * ( 1.0f - t ) * sq(t) * P2
                + cube(t) * P3;
        }

        public static Vector3 Point(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            return cube(1.0f - t) * P0
                + 3.0f * sq(1.0f - t) * t * P1
                + 3.0f * ( 1.0f - t ) * sq(t) * P2
                + cube(t) * P3;
        }

        public static Vector3 Point(Vector3 P0, Vector3 P1, Vector3 P2, float t)
        {
            return (1.0f - t) * (1.0f - t) * P0 + 2 * (1.0f - t) * t * P1 + t * t * P2;
        }

        public static Vector2 Point(Vector2 P0, Vector2 P1, Vector2 P2, float t)
        {
            return ( 1.0f - t ) * ( 1.0f - t ) * P0 + 2 * ( 1.0f - t ) * t * P1 + t * t * P2;
        }

        public static Vector3 Point(Vector3 P0, Vector3 P1, float t)
        {
            return (1.0f - t) * P0 + t * P1;
        }

        public static Vector2 Point(Vector2 P0, Vector2 P1, float t)
        {
            return ( 1.0f - t ) * P0 + t * P1;
        }

        public static Vector2 Tangent(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t)
        {
            Vector2 R = Point(Point(P1, P2, t), Point(P2, P3, t), t) - Point(Point(P0, P1, t), Point(P1, P2, t), t);
            R.Normalize();
            return R;
        }

        public static Vector3 Tangent(Vector3 P0, Vector3 P1, Vector3 P2, float t)
        {
            Vector3 result = Point(P1, P2, t) - Point(P0, P1, t);
            result.Normalize();
            return result;
        }

        public static Vector2 Tangent(Vector2 P0, Vector2 P1, Vector2 P2, float t)
        {
            Vector2 result = Point(P1, P2, t) - Point(P0, P1, t);
            result.Normalize();
            return result;
        }
    }
}
