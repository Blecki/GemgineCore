using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Common
{
    public class Grid<T> where T : new()
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public float binWidth { get; private set; }
        public float binHeight { get; private set; }

        private T[] tiles;

        public Grid(int width, int height, float binWidth, float binHeight)
        {
            this.width = (int)(width / binWidth);
            if (this.width * binWidth < width) this.width += 1;

            this.height = (int)(height / binHeight);
            if (this.height * binHeight < height) this.height += 1;

            this.binWidth = binWidth;
            this.binHeight = binHeight;

            tiles = new T[this.width * this.height];
            for (int i = 0; i < tiles.Length; ++i) tiles[i] = new T();
        }

        public int Normalize(int x, int y)
        {
            return (y * width) + x;
        }

        public T this[int x, int y]
        {
            get { return tiles[Normalize(x, y)]; }
            set { tiles[Normalize(x, y)] = value; }
        }

        public void forRect(int x, int y, int w, int h, Action<T, int, int> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                    func(this[_x, _y], _x, _y);
        }

        public A abortedForRect<A>(int x, int y, int w, int h, Func<T, int, int, A> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                {
                    var _a = func(this[_x, _y], _x, _y);
                    if (_a != null) return _a;
                }
            return default(A);
        }

        public void forWorldRect(float x, float y, float w, float h, Action<T, int, int> func)
		{
            var _w = (int)(System.Math.Ceiling((x + w) / binWidth) - System.Math.Floor(x / binWidth));
            var _h = (int)(System.Math.Ceiling((y + h) / binHeight) - System.Math.Floor(y / binHeight));
			forRect((int)(x / binWidth), (int)(y / binHeight), _w, _h, func);
		}

        public T worldIndex(float x, float y)
        {
            return this[(int)System.Math.Floor(x / binWidth), (int)System.Math.Floor(y / binHeight)];
        }

        public bool check(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool worldCheck(float x, float y)
        {
            return check((int)System.Math.Floor(x / binWidth), (int)System.Math.Floor(y / binHeight));
        }
    }
}
