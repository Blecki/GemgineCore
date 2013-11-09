using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace Gem.Epiphany
{
	internal struct Triangle
	{
		public Vector2 A;
		public Vector2 B;
		public Vector2 C;

		public Triangle(Triangle other)
		{
			A = other.A;
			B = other.B;
			C = other.C;
		}

		public Triangle(Vector2 A, Vector2 B, Vector2 C)
		{
			this.A = A;
			this.B = B;
			this.C = C;
		}

		/// <summary>
		/// I have no idea how this works. I think it's transforming vec into a 
		/// cartesian coordinate space defined by the triangle. u,v is the 
		/// transformed vec.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		internal bool IsInside(Vector2 vec)
		{
			Vector2 v2 = vec - A;
			Vector2 v1 = B - A;
			Vector2 v0 = C - A;

			float dot00 = Vector2.Dot(v0, v0);
			float dot01 = Vector2.Dot(v0, v1);
			float dot02 = Vector2.Dot(v0, v2);
			float dot11 = Vector2.Dot(v1, v1);
			float dot12 = Vector2.Dot(v1, v2);
			float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
			float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
			float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

			return ((u > 0) && (v > 0) && (u + v < 1));
		}

			
	}

	public class Decomposition
	{
		private static Polygon CreatePolygon(Triangle t)
		{
            var r = new Polygon();
            r.Add(t.A);
            r.Add(t.B);
            r.Add(t.C);
            return r;
		}

        /// <summary>
		/// Decomposes a non-convex polygon into a number of convex polygons, up
		/// to maxPolys (remaining pieces are thrown out, but the total number
		/// is returned, so the return value can be greater than maxPolys).
		///
		/// Each resulting polygon will have no more than maxVerticesPerPolygon
		/// vertices (set to b2MaxPolyVertices by default, though you can change
		/// this).
		/// 
		/// Returns -1 if operation fails (usually due to self-intersection of
		/// polygon).
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="results">The results.</param>
		/// <param name="maxPolys">The max polys.</param>
		/// <returns></returns>
        public static int DecomposeConvex(Polygon p, out List<Polygon> results, int maxPointsPerPolygon)
		{
            results = new List<Polygon>();

			if (p.Count < 3) return 0;

			Triangle[] triangulated;// = new Triangle[p.Count - 2];
			int triangleCount = TriangulatePolygon(p, out triangulated);
				
			if (triangleCount < 1)
			{
				//Still no luck?  Oh well...
				return -1;
			}
			int polygonCount = PolygonizeTriangles(triangulated, triangleCount, out results, maxPointsPerPolygon);
			return polygonCount;
		}

		private static float Cross(ref Vector2 value1, ref Vector2 value2)
		{
			return value1.X * value2.Y - value1.Y * value2.X;
		}

	    /// <summary>
        /// Triangulates a polygon using simple ear-clipping algorithm. Returns
        /// size of Triangle array unless the polygon can't be triangulated.
        /// This should only happen if the polygon self-intersects,
        /// though it will not _always_ return null for a bad polygon - it is the
        /// caller's responsibility to check for self-intersection, and if it
        /// doesn't, it should at least check that the return value is non-null
        /// before using. You're warned!
        ///
        /// Triangles may be degenerate, especially if you have identical points
        /// in the input to the algorithm.  Check this before you use them.
        ///
        /// This is totally unoptimized, so for large polygons it should not be part
        /// of the simulation loop.
        ///
        /// Returns:
        /// -1 if algorithm fails (self-intersection most likely)
        /// 0 if there are not enough vertices to triangulate anything.
        /// Number of triangles if triangulation was successful.
        ///
        /// results will be filled with results - ear clipping always creates vNum - 2
        /// or fewer (due to pinch point polygon snipping), so allocate an array of
        /// this size.
        /// </summary>
        /// <param name="xv">The xv.</param>
        /// <param name="yv">The yv.</param>
        /// <param name="vNum">The v num.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        internal static int TriangulatePolygon(Polygon _p, out Triangle[] results)
        {
			results = new Triangle[_p.Count - 2];

            if (_p.Count < 3)
                return 0;

            Polygon p = new Polygon(_p);

            //If the polygon has a pinch point (2 coincident verticies) split the polygon on it.
            Polygon pA, pB;
            Polygon pin = new Polygon(p);
            if (ResolvePinchPoint(pin, out pA, out pB))
            {
                Triangle[] mergeA = new Triangle[pA.Count];
                Triangle[] mergeB = new Triangle[pB.Count];
                int triangleCountA = TriangulatePolygon(pA, out mergeA);
                int triangleCountB = TriangulatePolygon(pB, out mergeB);
                if (triangleCountA == -1 || triangleCountB == -1)
                    return -1;
				//Copy pinch point sides into triangle buffer
                for (int i = 0; i < triangleCountA; ++i)
                    results[i] = new Triangle(mergeA[i]);
                for (int i = 0; i < triangleCountB; ++i)
                    results[triangleCountA + i] = new Triangle(mergeB[i]);
                return (triangleCountA + triangleCountB);
            }

            Triangle[] buffer = new Triangle[p.Count - 2];
            int bufferSize = 0;
			//Vector2[] rem = new Vector2[p.Count];
			//for (int i = 0; i < p.Count; ++i)
			//	rem[i] = p[i];

            while (p.Count > 3)
            {
                // Find an ear
                int earIndex = -1;
                float earMaxMinCross = -10.0f;

                for (int i = 0; i < p.Count; ++i)
                {
                    if (IsEar(i, p))
                    {
                        int lower = Remainder(i - 1, p.Count);
                        int upper = Remainder(i + 1, p.Count);
						Vector2 d1 = p[upper] - p[i];// new Vector2(xrem[upper] - xrem[i], yrem[upper] - yrem[i]);
						Vector2 d2 = p[i] - p[lower];// new Vector2(xrem[i] - xrem[lower], yrem[i] - yrem[lower]);
						Vector2 d3 = p[lower] - p[upper];// new Vector2(xrem[lower] - xrem[upper], yrem[lower] - yrem[upper]);

                        d1.Normalize();
                        d2.Normalize();
                        d3.Normalize();
                        float cross12 = System.Math.Abs(Cross(ref d1, ref d2));
                        float cross23 = System.Math.Abs(Cross(ref d2, ref d3));
                        float cross31 = System.Math.Abs(Cross(ref d3, ref d1));
                        //Find the maximum minimum angle
                        float minCross = System.Math.Min(cross12, System.Math.Min(cross23, cross31));
                        if (minCross > earMaxMinCross)
                        {
                            earIndex = i;
                            earMaxMinCross = minCross;
                        }

                        
                    }
                }

                // If we still haven't found an ear, we're screwed.
                // Note: sometimes this is happening because the
                // remaining points are collinear.  Really these
                // should just be thrown out without halting triangulation.
                if (earIndex == -1)
                {
                    for (int i = 0; i < bufferSize; i++)
                        results[i] = new Triangle(buffer[i]);

                    if (bufferSize > 0)
                        return bufferSize;

                    return -1;
                }

                
                // - add the clipped triangle to the result buffer
                int under = (earIndex == 0) ? (p.Count - 1) : (earIndex - 1);
                int over = (earIndex == p.Count - 1) ? 0 : (earIndex + 1);
                Triangle toAdd = new Triangle(p[earIndex], p[over], p[under]);//xrem[earIndex], yrem[earIndex], xrem[over], yrem[over], xrem[under], yrem[under]);
                buffer[bufferSize] = new Triangle(toAdd);
                ++bufferSize;

				// Clip off the ear:
				// - remove the ear tip from the list
				p.RemoveAt(earIndex);
            }

            Triangle tooAdd = new Triangle(p[0], p[1], p[2]);//xrem[1], yrem[1], xrem[2], yrem[2],
                                      //xrem[0], yrem[0]);
            buffer[bufferSize] = new Triangle(tooAdd);
            ++bufferSize;

            for (int i = 0; i < bufferSize; i++)
                results[i] = new Triangle(buffer[i]);

            return bufferSize;
        }

		/// <summary>
		/// Finds and fixes "pinch points," points where two polygon
		/// vertices are at the same point.
		/// If a pinch point is found, pin is broken up into poutA and poutB
		/// and true is returned; otherwise, returns false.
		/// Mostly for internal use.
		/// </summary>
		/// <param name="pin">The pin.</param>
		/// <param name="poutA">The pout A.</param>
		/// <param name="poutB">The pout B.</param>
		/// <returns></returns>
        private static bool ResolvePinchPoint(Polygon pin, out Polygon poutA, out Polygon poutB)
		{
			poutA = new Polygon();
			poutB = new Polygon();

			if (pin.Count < 3) return false;
            const float tolerance = .001f;
			bool hasPinchPoint = false;
			int pinchIndexA = -1;
			int pinchIndexB = -1;
			for (int i = 0; i < pin.Count; ++i)
			{
				for (int j = i + 1; j < pin.Count; ++j)
				{
					//Don't worry about pinch points where the points
					//are actually just dupe neighbors
					if (System.Math.Abs(pin[i].X - pin[j].X) < tolerance && System.Math.Abs(pin[i].Y - pin[j].Y) < tolerance 
						&& j != i + 1)
					{
						pinchIndexA = i;
						pinchIndexB = j;
						hasPinchPoint = true;
						break;
					}
				}
				if (hasPinchPoint) break;
			}
			if (hasPinchPoint)
			{
				int sizeA = pinchIndexB - pinchIndexA;
				if (sizeA == pin.Count) return false;//has dupe points at wraparound, not a problem here
				poutA.Clear();
				for (int i = 0; i < sizeA; ++i)
					poutA.Add(pin[Remainder(pinchIndexA + i, pin.Count)]);
				
				int sizeB = pin.Count - sizeA;
				poutB.Clear();
				for (int i = 0; i < sizeB; ++i)
					poutB.Add(pin[Remainder(pinchIndexB + i, pin.Count)]);
				
			}
			return hasPinchPoint;
		}

		private static int Remainder(int x, int modulus)
		{
			int rem = x % modulus;
			while (rem < 0)
			{
				rem += modulus;
			}
			return rem;
		}

		/// <summary>
        /// Checks if vertex i is the tip of an ear in polygon defined by xv[] and
        /// yv[].
        ///
        /// Assumes clockwise orientation of polygon...ick
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="xv">The xv.</param>
        /// <param name="yv">The yv.</param>
        /// <param name="xvLength">Length of the xv.</param>
        /// <returns>
        /// 	<c>true</c> if the specified i is ear; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsEar(int i, Polygon p)//Vector2[] v, int vLength)
        {
			Vector2 d0, d1;
            //float dx0, dy0, dx1, dy1;
            if (i >= p.Count || i < 0 || p.Count < 3)
            {
                return false;
            }
            int upper = i + 1;
            int lower = i - 1;
            if (i == 0)
            {
				d0 = p[0] - p[p.Count - 1];
				d1 = p[1] - p[0];
                lower = p.Count - 1;
            }
            else if (i == p.Count - 1)
            {
				d0 = p[i] - p[i - 1];
				d1 = p[0] - p[i];
                upper = 0;
            }
            else
            {
				d0 = p[i] - p[i - 1];
				d1 = p[i + 1] - p[i];
            }
            float cross = d0.X * d1.Y - d1.X * d0.Y;
            if (cross < 0)
                return false;
            Triangle myTri = new Triangle(p[i], p[upper], p[lower]);
            for (int j = 0; j < p.Count; ++j)
            {
                if (j == i || j == lower || j == upper)
                    continue;
                if (myTri.IsInside(p[j]))
                    return false;
            }
            return true;
        }

		/// <summary>
		/// Turns a list of triangles into a list of convex polygons. Very simple
		/// method - start with a seed triangle, keep adding triangles to it until
		/// you can't add any more without making the polygon non-convex.
		///
		/// Returns an integer telling how many polygons were created.  Will fill
		/// polys array up to polysLength entries, which may be smaller or larger
		/// than the return value.
		/// 
		/// Takes O(N///P) where P is the number of resultant polygons, N is triangle
		/// count.
		/// 
		/// The final polygon list will not necessarily be minimal, though in
		/// practice it works fairly well.
		/// </summary>
		/// <param name="triangulated">The triangulated.</param>
		/// <param name="triangulatedLength">Length of the triangulated.</param>
		/// <param name="polys">The polys.</param>
		/// <param name="polysLength">Length of the polys.</param>
		/// <returns></returns>
		private static int PolygonizeTriangles(
			Triangle[] triangulated, 
			int triangulatedLength,
            out List<Polygon> polys, 
			int maxVerticiesPerPolygon)
		{
			int polyIndex = 0;
            polys = new List<Polygon>();

			if (triangulatedLength <= 0)
				return 0;

			bool[] covered = new bool[triangulatedLength];
			for (int i = 0; i < triangulatedLength; ++i)
			{
				covered[i] = false;
				//Check here for degenerate triangles
				if ( (triangulated[i].A == triangulated[i].B)
					 || (triangulated[i].B == triangulated[i].C)
					 || (triangulated[i].C == triangulated[i].A))
				{
					covered[i] = true;
				}
			}

			bool notDone = true;
			while (notDone)
			{
				int currTri = -1;

				//Find first not-covered triangle
				for (int i = 0; i < triangulatedLength; ++i)
				{
					if (covered[i])	continue;
					currTri = i;
					break;
				}

				//If all triangles are covered, we are done.
				if (currTri == -1)
					notDone = false;
				else
				{
                    Polygon poly = CreatePolygon(triangulated[currTri]);
					covered[currTri] = true;
					int index = 0;
					for (int i = 0; i < 2 * triangulatedLength; ++i, ++index)
					{
						while (index >= triangulatedLength) index -= triangulatedLength;
						if (covered[index])
						{
							continue;
						}
                        Polygon newP = Add(poly, triangulated[index]);
						if (newP == null)
						{                                 // is this right
							continue;
						}
						if (newP.Count > maxVerticiesPerPolygon)
						{
							newP = null;
							continue;
						}
						if (IsConvex(newP))
						{ //Or should it be IsUsable?  Maybe re-write IsConvex to apply the angle threshold from Box2d
                            poly = new Polygon(newP);
							newP = null;
							covered[index] = true;
						}
						else
						{
							newP = null;
						}
					}
					//if (polyIndex < polysLength)
					//{
						//poly.MergeParallelEdges(angularSlop);
						//If identical points are present, a triangle gets
						//borked by the MergeParallelEdges function, hence
						//the vertex number check
					if (poly.Count >= 3)
					{
						polys.Add(poly);
						//polys[polyIndex] = new Polygon(poly);
						polyIndex++;
					}
						//else printf("Skipping corrupt poly\n");
					//}
					//if (poly.Count >= 3) polyIndex++; //Must be outside (polyIndex < polysLength) test
				}
			}
			return polyIndex;
		}

		/// <summary>
		/// Tries to add a triangle to the polygon. Returns null if it can't connect
		/// properly, otherwise returns a pointer to the new Polygon. Assumes bitwise
		/// equality of joined vertex positions.
		///
		/// For internal use.
		/// </summary>
		/// <param name="t">The triangle to add.</param>
		/// <returns></returns>
        private static Polygon Add(Polygon p, Triangle t)
		{
			//		float32 equalTol = .001f;
			// First, find vertices that connect
			int firstP = -1;
			int firstT = -1;
			int secondP = -1;
			int secondT = -1;
            for (int i = 0; i < p.Count; i++)
			{
				if (t.A == p[i])
				{
					if (firstP == -1)
					{
						firstP = i;
						firstT = 0;
					}
					else
					{
						secondP = i;
						secondT = 0;
					}
				}
				else if (t.B == p[i])
				{
					if (firstP == -1)
					{
						firstP = i;
						firstT = 1;
					}
					else
					{
						secondP = i;
						secondT = 1;
					}
				}
				else if (t.C == p[i])
				{
					if (firstP == -1)
					{
						firstP = i;
						firstT = 2;
					}
					else
					{
						secondP = i;
						secondT = 2;
					}
				}
			}
			// Fix ordering if first should be last vertex of poly
            if (firstP == 0 && secondP == p.Count - 1)
			{
                firstP = p.Count - 1;
				secondP = 0;
			}

			// Didn't find it
			if (secondP == -1)
			{
				return null;
			}

			// Find tip index on triangle
			int tipT = 0;
			if (tipT == firstT || tipT == secondT)
				tipT = 1;
			if (tipT == firstT || tipT == secondT)
				tipT = 2;

            Polygon result = new Polygon(p);
			if (tipT == 0)
				result.Insert(firstP + 1, t.A);
			else if (tipT == 1)
				result.Insert(firstP + 1, t.B);
			else if (tipT == 2)
				result.Insert(firstP + 1, t.C);

			return result;
		}

		/// <summary>
		/// Assuming the polygon is simple, checks if it is convex.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is convex; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsConvex(Polygon p)
		{
			bool isPositive = false;
			for (int i = 0; i < p.Count; ++i)
			{
                int lower = (i == 0) ? (p.Count - 1) : (i - 1);
				int middle = i;
                int upper = (i == p.Count - 1) ? (0) : (i + 1);
				Vector2 d0 = p[middle] - p[lower];
				Vector2 d1 = p[upper] - p[middle];
				float cross = d0.X * d1.Y - d1.X * d0.Y;

				// Cross product should have same sign
				// for each vertex if poly is convex.
				bool newIsP = (cross >= 0) ? true : false;
				if (i == 0)
				{
					isPositive = newIsP;
				}
				else if (isPositive != newIsP)
				{
					return false;
				}
			}
			return true;
		}


	}
}
