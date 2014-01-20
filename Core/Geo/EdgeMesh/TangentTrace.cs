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
		private static float CrossZ(Vector3 A, Vector3 B)
		{
			return (B.Y * A.X) - (B.X * A.Y);
		}

		private static bool SegmentWithSegement2D(Vector3 A0, Vector3 A1, Vector3 B0, Vector3 B1, out Vector3 IntersectionPoint)
		{
			Vector3 U = A1 - A0;
			Vector3 V = B1 - B0;
			Vector3 W = A0 - B0;
			float D = CrossZ(U, V);

			IntersectionPoint = A0;

			if (System.Math.Abs(D) < 0.01)
				return false; //Segments are parallel

			float SI = CrossZ(V, W) / D;
			if (SI < 0 || SI > 1)
				return false; //Intersection is beyond limits of line segment

			float TI = CrossZ(U, W) / D;
			if (TI < 0 || TI > 1)
				return false; //Intersection is beyond limits of line segment

			IntersectionPoint = A0 + (SI * U);
			return true;
		}

		public bool CanTrace(Vector3 from, Vector3 to, Action<Vector3> DebugIntersectionPoints = null)
		{
			var startFace = FaceAt(from);
			EMFace lastFace = null;

			while (startFace != null)
			{
				if (IsPointOnFace(to, startFace)) return true;

				float intersectionDistance = 0.0f;
				EMEdge intersectionEdge = null;
				
				//Find the most distant edge intersection
				foreach (var edge in startFace.edges)
				{
					Vector3 iPoint;
					if (SegmentWithSegement2D(this.Verticies[edge.Verticies[0]], this.Verticies[edge.Verticies[1]], from, to,
						out iPoint))
					{
						var distance = (iPoint - from).LengthSquared();
						if (
							(intersectionEdge == null || distance > intersectionDistance) &&
							(OtherNeighbor(edge, startFace) != lastFace))
						{
							intersectionEdge = edge;
							intersectionDistance = distance;
						}

						if (DebugIntersectionPoints != null) DebugIntersectionPoints(iPoint);
					}
				}

				if (intersectionEdge == null)
					return false;

				var nextFace = OtherNeighbor(intersectionEdge, startFace);
				if (nextFace == null) return false; //We reached an edge with no neighbor.

				lastFace = startFace;
				startFace = nextFace;
			}

			return false;
		}
    }
}
