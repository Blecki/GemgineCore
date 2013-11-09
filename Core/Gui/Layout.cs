using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public static class Layout
    {
        public static Rectangle CenterItem(Rectangle What, Rectangle Area)
        {
            return new Rectangle(
                Area.X + (Area.Width / 2) - (What.Width / 2),
                Area.Y + (Area.Height / 2) - (What.Height / 2),
                What.Width, What.Height);
        }

        [Flags]
        public enum Placements
        {
            Top = 1,
            Right = 2,
            Bottom = 4,
            Left = 8,
            TopRight = Top | Right,
            TopLeft = Top | Left,
            BottomRight = Bottom | Right,
            BottomLeft = Bottom | Left,
        }

        public static Rectangle FloatItem(Rectangle What, Rectangle Within, Placements Placement, int Padding)
        {
            if ((Placement & Placements.Top) == Placements.Top)
                What.Y = Within.Y + Padding;
            if ((Placement & Placements.Bottom) == Placements.Bottom)
                What.Y = Within.Y + Within.Height - What.Height - Padding;
            if ((Placement & Placements.Left) == Placements.Left)
                What.X = Within.X + Padding;
            if ((Placement & Placements.Right) == Placements.Right)
                What.X = Within.X + Within.Width - What.Width - Padding;
            return What;
        }
    }
}
