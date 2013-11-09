using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Gem.Math;

namespace Gem.Math
{
    public class OBB
    {
        private Vector2 _center;
        private Vector2 _extents;
        private float _angle;

        public Vector2 Center { get { return _center; } set { _center = value; Valid = false; } }
        public Vector2 Extents { get { return _extents; } set { _extents = value; Valid = false; } }
        public float Angle { get { return _angle; } set { _angle = value; Valid = false; } }

        private bool Valid = false;
        private Vector2[] Points = new Vector2[4];

        public OBB(Vector2 Center, Vector2 Extents)
        {
            this.Center = Center;
            this.Extents = Extents;
        }

        public OBB() { }

        public Vector2[] GetPoints()
        {
            if (Valid) return Points;

            Points[0] = new Vector2(-Extents.X, -Extents.Y);
            Points[1] = new Vector2(-Extents.X, Extents.Y);
            Points[2] = new Vector2(Extents.X, Extents.Y);
            Points[3] = new Vector2(Extents.X, -Extents.Y);

            Matrix M = Matrix.CreateRotationZ(Angle);
            for (int i = 0; i < 4; ++i) { Points[i] = Vector2.Transform(Points[i], M); Points[i] += Center; }

            Valid = true;
            return Points;
        }

        static private bool AxisSeperatedTest(Vector2 Axis, Vector2[] APoints, Vector2[] BPoints)
        {
            Axis.Normalize();
            float AMin, AMax, BMin, BMax;

            AMin = AMax = Vector2.Dot(Axis, APoints[0]);
            for (int i = 1; i < 4; ++i)
            {
                float P = Vector2.Dot(Axis, APoints[i]);
                if (P < AMin) AMin = P;
                if (P > AMax) AMax = P;
            }

            BMin = BMax = Vector2.Dot(Axis, BPoints[0]);
            for (int i = 1; i < 4; ++i)
            {
                float P = Vector2.Dot(Axis, BPoints[i]);
                if (P < BMin) BMin = P;
                if (P > BMax) BMax = P;
            }

            if (AMin >= BMax || BMin >= AMax) return true;
            return false;
        }

        static public bool Intersect(OBB A, OBB B)
        {
            var APoints = A.GetPoints();
            var BPoints = B.GetPoints();

            if (AxisSeperatedTest(Vector.Perpendicular(APoints[1] - APoints[0]), APoints, BPoints)) return false;
            if (AxisSeperatedTest(Vector.Perpendicular(APoints[2] - APoints[1]), APoints, BPoints)) return false;
            if (AxisSeperatedTest(Vector.Perpendicular(BPoints[1] - BPoints[0]), APoints, BPoints)) return false;
            if (AxisSeperatedTest(Vector.Perpendicular(BPoints[2] - BPoints[1]), APoints, BPoints)) return false;
            return true;
        }
    }
}
