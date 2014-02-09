using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public struct VirtualLayout
    {
        Rectangle VirtualArea;
		Rectangle RealArea;

        public VirtualLayout(Rectangle VirtualArea, Rectangle RealArea)
        {
			this.VirtualArea = VirtualArea;
			this.RealArea = RealArea;
        }

		public Rectangle RealRectangle(Rectangle VirtualRectangle)
		{
			//Simple ratios: VR.W / VA.W == RR.W / RA.W
			//Solved for RR.W: RR.W == (VR.W / VA.W) * RA.W

			VirtualRectangle.X -= VirtualArea.X;
			VirtualRectangle.Y -= VirtualArea.Y;

			float rW = ((float)VirtualRectangle.Width / (float)VirtualArea.Width) * (float)RealArea.Width;
			float rH = ((float)VirtualRectangle.Height / (float)VirtualArea.Height) * (float)RealArea.Height;
			float rX = ((float)VirtualRectangle.X / (float)VirtualArea.Width) * (float)RealArea.Width;
			float rY = ((float)VirtualRectangle.Y / (float)VirtualArea.Height) * (float)RealArea.Height;

			return new Rectangle((int)(rX + RealArea.X), (int)(rY + RealArea.Y), (int)rW, (int)rH);
		}

    }
}
