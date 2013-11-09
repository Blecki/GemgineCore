using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public struct BorderLayout
    {
        public enum Sides
        {
            North,
            East,
            South,
            West,
            Center
        }

        Rectangle CoverageArea;
        int Padding;

        public BorderLayout(Rectangle Area, int Padding)
        {
            CoverageArea = Layout.CenterItem(new Rectangle(0,0,Area.Width - 2 * Padding, Area.Height - 2 * Padding), Area);
            this.Padding = Padding;            
        }

        public Rectangle PositionItem(Sides side, int dim)
        {
            Rectangle R;

            switch (side)
            {
                case Sides.North:
                    R = new Rectangle(CoverageArea.X, CoverageArea.Y, CoverageArea.Width, dim);
                    CoverageArea.Y += dim + Padding; CoverageArea.Height -= dim + Padding;
                    break;
                case Sides.East:
                    R = new Rectangle(CoverageArea.X + CoverageArea.Width - dim, CoverageArea.Y, dim, CoverageArea.Height);
                    CoverageArea.Width -= dim + Padding;
                    break;
                case Sides.South:
                    R = new Rectangle(CoverageArea.X, CoverageArea.Y + CoverageArea.Height - dim, CoverageArea.Width, dim);
                    CoverageArea.Height -= dim + Padding;
                    break;
                case Sides.West:
                    R = new Rectangle(CoverageArea.X, CoverageArea.Y, dim, CoverageArea.Height);
                    CoverageArea.X += dim + Padding; CoverageArea.Width -= dim + Padding;
                    break;
                case Sides.Center:
                    R = CoverageArea;
                    CoverageArea = new Rectangle(0, 0, 0, 0);
                    break;
                default :
                    R = new Rectangle(0, 0, 0, 0);
                    break;
            }

            return R;
        }
    }
}
