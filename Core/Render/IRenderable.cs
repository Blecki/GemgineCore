using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Render
{
	public enum RenderMode
	{
		Normal,
		MousePick
	}

    public interface IRenderable
    {
        void PreDraw(float elapsedSeconds, GraphicsDevice device, RenderContext context);
        void DrawEx(RenderContext context, RenderMode Mode);
        void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug);
		void SetHilite(bool hilited);
    }
}
