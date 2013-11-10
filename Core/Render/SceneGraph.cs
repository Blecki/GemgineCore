using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class SceneGraphRoot : Render.IRenderable
    {
        public ISceneNode rootNode = null;

        public SceneGraphRoot(ISceneNode root)
        {
            rootNode = root;
        }

        void IRenderable.PreDraw(float elapsedSeconds, GraphicsDevice device, RenderContext context)
        {
            rootNode.UpdateWorldTransform(Matrix.Identity);   
        }

        void IRenderable.DrawEx(RenderContext context)
        {
            rootNode.Draw(context);
        }

        void IRenderable.CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            rootNode.CalculateLocalMouse(mouseRay, debug);
        }
    }
}
