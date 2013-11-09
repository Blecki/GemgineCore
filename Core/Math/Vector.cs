using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
	public class Vector
	{
        static public Vector2 DropZ(Vector3 A) { return new Vector2(A.X, A.Y); }

		static public float CrossZ(Vector2 A, Vector2 B)
		{
			return (B.Y * A.X) - (B.X * A.Y);
		}

        static public float CrossZ(Vector3 A, Vector3 B)
        {
            return (B.Y * A.X) - (B.X * A.Y);
        }

        static public float CrossZSign(Vector2 A, Vector2 B)
        {
            if (CrossZ(A, B) < 0) return -1.0f;
            else return 1.0f;
        }

		static public float AngleBetweenVectors(Vector2 A, Vector2 B)
		{
			A.Normalize();
			B.Normalize();
			float DotProduct = Vector2.Dot(A, B);
			DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);
			float Angle = (float)System.Math.Acos(DotProduct);
			if (CrossZ(A, B) < 0) return -Angle;
			return Angle;
		}

        static public float AngleBetweenVectors(Vector3 A, Vector3 B)
        {
            A.Normalize();
            B.Normalize();
            float DotProduct = Vector3.Dot(A, B);
            DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);
            float Angle = (float)System.Math.Acos(DotProduct);
            if (CrossZ(A, B) < 0) return -Angle;
            return Angle;
        }

		static public Vector2 Perpendicular(Vector2 A)
		{
			return new Vector2(A.Y, -A.X);
		}

		//Returns the NORMALIZED normal of the edge
		static public Vector2 EdgeNormal(Vector2 P0, Vector2 P1)
		{
			Vector2 R = P1 - P0;
			R.Normalize();
			return Perpendicular(R);
		}

		static public Vector2 Normalize(Vector2 Vec)
		{
			Vec.Normalize();
			return Vec;
		}

		public static Vector3 ConvertToVector(string p)
		{
			System.Text.RegularExpressions.Regex Elems = new System.Text.RegularExpressions.Regex(@"^(?<X>\S+)\s(?<Y>\S+)\s(?<Z>\S+)$");
			System.Text.RegularExpressions.Match Match = Elems.Match(p);
			if (!Match.Success) throw new System.FormatException();

			return new Vector3(
				Convert.ToSingle(Match.Groups["X"].Value),
				Convert.ToSingle(Match.Groups["Y"].Value),
				Convert.ToSingle(Match.Groups["Z"].Value));
		}

		public static Vector2 ConvertToVector2(String str)
		{
			System.Text.RegularExpressions.Regex Elems = new System.Text.RegularExpressions.Regex(@"^(?<X>\S+)\s(?<Y>\S+)$");
			System.Text.RegularExpressions.Match Match = Elems.Match(str);
			if (!Match.Success) throw new System.FormatException();
			return new Vector2(
				Convert.ToSingle(Match.Groups["X"].Value),
				Convert.ToSingle(Match.Groups["Y"].Value));
		}

		public static Vector2 ProjectAOntoB(Vector2 A, Vector2 B)
		{
			B.Normalize();
			return Vector2.Dot(A, B) * B;
		}

        public static Vector3 ProjectAOntoB(Vector3 A, Vector3 B)
        {
            B.Normalize();
            return Vector3.Dot(A, B) * B;
        }

		public static bool AlmostZero(Vector2 A)
		{
			return (Math.Utility.AlmostZero(A.X) && Math.Utility.AlmostZero(A.Y));
		}

        public static void RoundVector(ref Vector2 vec, float divisions)
        {
            vec.X = (float)System.Math.Round(vec.X * divisions) / divisions;
            vec.Y = (float)System.Math.Round(vec.Y * divisions) / divisions;
        }

        public static void FloorVector(ref Vector2 vec, float divisions = 1.0f)
        {
            vec.X = (float)System.Math.Floor(vec.X * divisions) / divisions;
            vec.Y = (float)System.Math.Floor(vec.Y * divisions) / divisions;
        }
	}
}
