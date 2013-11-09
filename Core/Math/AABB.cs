using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace Gem.Math
{
	public class AABB
	{
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Vector2 Min { get { return new Vector2(X, Y); } }
        public Vector2 Max { get { return new Vector2(X + Width, Y + Height); } }
        public Vector2 Center { get { return new Vector2(X + Width / 2, Y + Height / 2); } }

		public AABB(float X, float Y, float Width, float Height)
		{
			this.X = X;
			this.Y = Y;
			this.Width = Width;
			this.Height = Height;
		}

		public AABB(AABB Other)
		{
			this.X = Other.X;
			this.Y = Other.Y;
			this.Width = Other.Width;
			this.Height = Other.Height;
		}

        public AABB() { }

		static public bool Intersect(AABB A, AABB B)
		{
			if (B.X + B.Width < A.X) return false;
			if (B.X > A.X + A.Width) return false;
			if (B.Y + B.Height < A.Y) return false;
			if (B.Y > A.Y + A.Height) return false;
			return true;
		}

		static public bool Inside(ref AABB A, ref Vector2 B)
		{
			if (B.X < A.X) return false;
			if (B.X >= A.X + A.Width) return false;
			if (B.Y < A.Y) return false;
			if (B.Y >= A.Y + A.Height) return false;
			return true;
		}
	}
}