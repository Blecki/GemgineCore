using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public struct GridLayout
    {
        Rectangle CoverageArea;
        int GridX, GridY;
        int NextX, NextY;

        public GridLayout(Rectangle Area, int X, int Y)
        {
            CoverageArea = Area;
            NextX = 0;
            NextY = 0;
            GridX = X;
            GridY = Y;
        }

        public Rectangle PositionItem()
        {
            var RW = CoverageArea.Width / GridX;
            var RH = CoverageArea.Height / GridY;
            Rectangle R = new Rectangle(CoverageArea.X + (NextX * RW), CoverageArea.Y + (NextY * RH), RW, RH);
            NextX += 1;
            if (NextX >= GridX)
            {
                NextX = 0;
                NextY += 1;
            }

            return R;
        }

    }
}
