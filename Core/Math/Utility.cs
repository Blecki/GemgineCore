using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
	public class Utility
	{
		public static bool AlmostZero(float f)
		{
			return NearlyEqual(f, 0.0f);
		}

		public static bool NearlyEqual(float A, float B)
		{
			if (A > B + Epsilon) return false;
			if (A < B - Epsilon) return false;
			return true;
		}

		public static float Epsilon = 0.0001f;
	}
}
