using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
	public partial class Intersection
	{

		public static bool PointInsidePolygon(List<Vector2> points, Vector2 point)
		{
			for (int i = 0; i < points.Count; ++i)
			{
				Vector2 first = points[i];
				Vector2 second = (i == points.Count - 1) ? points[0] : points[i + 1];
				if (Vector.CrossZ(second - first, point - first) <= 0.0f) return false;
			}

			return true;
		}

		public static bool PointInPolygonAngle(List<Vector2> polygon, Vector2 point)
		{
			double angle = 0;

			// Iterate through polygon's edges
			for (int i = 0; i < polygon.Count; i++)
			{
				/*
				p1.h = polygon[i].h - p.h;
				p1.v = polygon[i].v - p.v;
				p2.h = polygon[(i + 1) % n].h - p.h;
				p2.v = polygon[(i + 1) % n].v - p.v;
				*/
				// Get points
				Vector2 p1 = polygon[i] - point;
				Vector2 p2 = polygon[i == polygon.Count - 1 ? 0 : i + 1] - point;

				angle += Math.Vector.AngleBetweenVectors(p1, p2);
			}

			if (System.Math.Abs(angle) < System.Math.PI - Math.Utility.Epsilon)
			{
				return false;
			}

			return true;
		}
	}
}