using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Common
{
    public class Direction
    {
        public const int North = 0;
        public const int East = 1;
        public const int South = 2;
        public const int West = 3;
    
        public static int Rotate(int Dir, int A)
        {
            int R = Dir + A;
            if (R > 3) R -= 4;
            if (R < 0) R += 4;
            return R;
        }

        public static int Opposite(int Dir)
        {
            return Rotate(Dir, 2);
        }

        public static void Neighbor(int Dir, int x, int y, out int _x, out int _y)
        {
            switch (Dir)
            {
                case Direction.North:
                    _x = x;
                    _y = y - 1;
                    break;
                case Direction.East:
                    _x = x + 1;
                    _y = y;
                    break;
                case Direction.South:
                    _x = x;
                    _y = y + 1;
                    break;
                case Direction.West:
                    _x = x - 1;
                    _y = y;
                    break;
                default:
                    _x = x;
                    _y = y;
                    break;
            }
        }


    }
}