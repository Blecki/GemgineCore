using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render.SceneGraph
{
    public class Root : Render.IRenderable
    {
        public ISceneNode rootNode = null;

        public Root(ISceneNode root)
        {
            rootNode = root;
        }

        void IRenderable.PreDraw(float elapsedSeconds, GraphicsDevice device, RenderContext context)
        {
            rootNode.UpdateWorldTransform(Matrix.Identity);   
        }

        void IRenderable.DrawEx(RenderContext context, RenderMode mode)
        {
            rootNode.Draw(context);
        }

        void IRenderable.CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            rootNode.CalculateLocalMouse(mouseRay, debug);
        }

		public void SetHilite(bool hilited) { }
    }
}
