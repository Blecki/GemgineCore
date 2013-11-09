using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
	public partial class Intersection
	{
		public static bool LineWithLine(Vector2 A0, Vector2 A1, Vector2 B0, Vector2 B1, out Vector2 IntersectionPoint)
		{
			Vector2 U = A1 - A0;
			Vector2 V = B1 - B0;
			Vector2 W = A0 - B0;

			float D = Vector.CrossZ(U, V);

			IntersectionPoint = A0;

			if (Utility.AlmostZero(System.Math.Abs(D)))
				return false; //parallel

			float SI = Vector.CrossZ(V, W) / D;

			IntersectionPoint = A0 + (U * SI);
			return true;
		}

		public static bool LineWithSegement(Vector2 A0, Vector2 A1, Vector2 B0, Vector2 B1, out Vector2 IntersectionPoint)
		{
			Vector2 U = A1 - A0;
			Vector2 V = B1 - B0;
			Vector2 W = A0 - B0;
			float D = Vector.CrossZ(U, V);

			IntersectionPoint = A0;

			if (Utility.AlmostZero(System.Math.Abs(D)))
				return false; //Parallel

			float SI = Vector.CrossZ(V, W) / D;

			float TI = Vector.CrossZ(U, W) / D;
			if (TI < 0 || TI > 1)
				return false; //Intersection is beyond limits of line segment

			IntersectionPoint = A0 + (SI * U);
			return true;
		}

		public static Vector2 ClosestPointOnLineToPoint(Vector2 A0, Vector2 A1, Vector2 B)
		{
			Vector2 Result;
			LineWithLine(A0, A1, B, B + Vector.Perpendicular(A1 - A0), out Result);
			return Result;
		}

		public static bool SegmentWithSegement(Vector2 A0, Vector2 A1, Vector2 B0, Vector2 B1, out Vector2 IntersectionPoint)
		{
			Vector2 U = A1 - A0;
			Vector2 V = B1 - B0;
			Vector2 W = A0 - B0;
			float D = Vector.CrossZ(U, V);

			IntersectionPoint = A0;

			if (Utility.AlmostZero(System.Math.Abs(D)))
				return false; //Segments are parallel

			float SI = Vector.CrossZ(V, W) / D;
			if (SI < 0 || SI > 1)
				return false; //Intersection is beyond limits of line segment

			float TI = Vector.CrossZ(U, W) / D;
			if (TI < 0 || TI > 1)
				return false; //Intersection is beyond limits of line segment

			IntersectionPoint = A0 + (SI * U);
			return true;
		}

		public static Vector2 ClosestPointOnSegmentToPoint(Vector2 A0, Vector2 A1, Vector2 B)
		{
			Vector2 B0 = B;
			Vector2 B1 = B + Vector.Perpendicular(A1 - A0);

			Vector2 U = A1 - A0;
			Vector2 V = B1 - B0;
			Vector2 W = A0 - B0;
			float D = Vector.CrossZ(U, V);

			float SI = Vector.CrossZ(V, W) / D;
			if (SI < 0) return A0;
			if (SI > 1) return A1;
			return A0 + (SI * U);
			

		}

		public static bool PolygonWithAABB(List<Vector2> points, AABB AABB)
		{
			Vector2[] P = new Vector2[4];
			P[0] = new Vector2(AABB.X, AABB.Y);
			P[1] = new Vector2(AABB.X + AABB.Width + 1, AABB.Y);
			P[2] = new Vector2(AABB.X + AABB.Width + 1, AABB.Y + AABB.Height + 1);
			P[3] = new Vector2(AABB.X, AABB.Y + AABB.Height + 1);

			for (int v = 0; v < points.Count; ++v)
			{
				Vector2 B = points[v];
				Vector2 A = points[v == 0 ? points.Count - 1 : v - 1];

				Vector2 Edge = B - A;
				Vector2 Normal = new Vector2(-Edge.Y, Edge.X);

				float dotMin1, dotMax1, dotMin2, dotMax2;

				dotMin1 = dotMax1 = Vector2.Dot(Normal, points[0]);
				for (int i = 1; i < points.Count; ++i)
				{
					float dot = Vector2.Dot(Normal, points[i]);
					if (dot < dotMin1) dotMin1 = dot;
					if (dot > dotMax1) dotMax1 = dot;
				}

				dotMin2 = dotMax2 = Vector2.Dot(Normal, P[0]);
				for (int i = 1; i < 4; ++i)
				{
					float dot = Vector2.Dot(Normal, P[i]);
					if (dot < dotMin2) dotMin2 = dot;
					if (dot > dotMax2) dotMax2 = dot;
				}

				if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;
			}

			for (int v = 0; v < 4; ++v)
			{
				Vector2 B = P[v];
				Vector2 A = P[v == 0 ? 3 : v - 1];

				Vector2 Edge = B - A;
				Vector2 Normal = new Vector2(-Edge.Y, Edge.X);

				float dotMin1, dotMax1, dotMin2, dotMax2;

				dotMin1 = dotMax1 = Vector2.Dot(Normal, points[0]);
				for (int i = 1; i < points.Count; ++i)
				{
					float dot = Vector2.Dot(Normal, points[i]);
					if (dot < dotMin1) dotMin1 = dot;
					if (dot > dotMax1) dotMax1 = dot;
				}

				dotMin2 = dotMax2 = Vector2.Dot(Normal, P[0]);
				for (int i = 1; i < 4; ++i)
				{
					float dot = Vector2.Dot(Normal, P[i]);
					if (dot < dotMin2) dotMin2 = dot;
					if (dot > dotMax2) dotMax2 = dot;
				}

				if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;
			}

			return true;
		}

		public static bool FuzzySegmentWithAABB(float Fuzz, Vector2 P0, Vector2 P1, AABB AABB)
		{
			AABB.X += Fuzz;
			AABB.Y += Fuzz;
			AABB.Width -= 2 * Fuzz;
			AABB.Height -= 2 * Fuzz;
			return SegmentWithAABB(P0, P1, AABB);
		}

		public static bool SegmentWithAABB(Vector2 P0, Vector2 P1, AABB AABB)
		{
			Vector2[] P = new Vector2[4];
			P[0] = new Vector2(AABB.X, AABB.Y);
			P[1] = new Vector2(AABB.X + AABB.Width + 1, AABB.Y);
			P[2] = new Vector2(AABB.X + AABB.Width + 1, AABB.Y + AABB.Height + 1);
			P[3] = new Vector2(AABB.X, AABB.Y + AABB.Height + 1);

			
				Vector2 B = P1;
				Vector2 A = P0;

				Vector2 Edge = B - A;
				Vector2 Normal = new Vector2(-Edge.Y, Edge.X);

				float dotMin1, dotMax1, dotMin2, dotMax2;

				dotMin1 = dotMax1 = Vector2.Dot(Normal, P0);
				
				float dot = Vector2.Dot(Normal, P1);
				if (dot < dotMin1) dotMin1 = dot;
				if (dot > dotMax1) dotMax1 = dot;
				
				dotMin2 = dotMax2 = Vector2.Dot(Normal, P[0]);
				for (int i = 1; i < 4; ++i)
				{
					dot = Vector2.Dot(Normal, P[i]);
					if (dot < dotMin2) dotMin2 = dot;
					if (dot > dotMax2) dotMax2 = dot;
				}

				if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;

			for (int v = 0; v < 4; ++v)
			{
				B = P[v];
				A = P[v == 0 ? 3 : v - 1];

				Edge = B - A;
				Normal = new Vector2(-Edge.Y, Edge.X);

				//float dotMin1, dotMax1, dotMin2, dotMax2;

				dotMin1 = dotMax1 = Vector2.Dot(Normal, P0);
				
					dot = Vector2.Dot(Normal, P1);
					if (dot < dotMin1) dotMin1 = dot;
					if (dot > dotMax1) dotMax1 = dot;

				dotMin2 = dotMax2 = Vector2.Dot(Normal, P[0]);
				for (int i = 1; i < 4; ++i)
				{
					dot = Vector2.Dot(Normal, P[i]);
					if (dot < dotMin2) dotMin2 = dot;
					if (dot > dotMax2) dotMax2 = dot;
				}

				if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;
			}

			return true;

		}

        public static bool CircleWithAABB(Circle circle, AABB aabb)
        {
            var circleDistance = new Vector2(
                System.Math.Abs(circle.position.X - aabb.Center.X),
                System.Math.Abs(circle.position.Y - aabb.Center.Y));

            if (circleDistance.X > (aabb.Width / 2 + circle.radius)) return false;
            if (circleDistance.Y > (aabb.Height / 2 + circle.radius)) return false;

            if (circleDistance.X <= (aabb.Width / 2)) return true;
            if (circleDistance.Y <= (aabb.Height / 2)) return true;

            var  cornerDistance_sq = 
                (circleDistance.X - aabb.Width / 2) * (circleDistance.X - aabb.Width / 2) +
                                 (circleDistance.Y - aabb.Height / 2) * (circleDistance.Y - aabb.Height / 2);

            return (cornerDistance_sq <= (circle.radius * circle.radius));
        }

	}
}
