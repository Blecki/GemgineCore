using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public struct FlowLayout
    {
        Rectangle CoverageArea;
        int NextX, NextY, CurrentY;
        int Padding;

        public FlowLayout(Rectangle Area, int Padding)
        {
            CoverageArea = Area;
            NextX = Padding;
            NextY = Padding;
            CurrentY = Padding;
            this.Padding = Padding;            
        }

        public Rectangle PositionItem(int W, int H)
        {
            Rectangle R = new Rectangle(NextX, CurrentY, W, H);
            if (R.Right + Padding > CoverageArea.Width)
            {
                R.X = Padding;
                NextX = Padding;
                R.Y = NextY;
                CurrentY = NextY;
            }

            NextX = NextX + R.Width + Padding;
            if (NextY < R.Bottom + Padding) NextY = R.Bottom + Padding;

            R.X += CoverageArea.X;
            R.Y += CoverageArea.Y;

            return R;
        }

        public void ForceNewLine()
        {
            NextX = Padding;
            CurrentY = NextY + Padding;
        }
    }
}
