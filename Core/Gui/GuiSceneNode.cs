using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Common;
using Gem.Gui;

namespace Gem.Gui
{
    public class SceneNode : Render.ISceneNode
    {
        public Renderable Renderable;

        public UIItem Root { get { return Renderable.uiRoot; } }
        public Euler Orientation;

        private Matrix worldTransformation = Matrix.Identity;

        public SceneNode(Renderable Renderable, Euler Euler = null)
        {
            this.Orientation = Euler;
            if (this.Orientation == null) this.Orientation = new Euler();
            this.Renderable = Renderable;
        }

        void Render.ISceneNode.UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m * Orientation.Transform;
        }

        void Render.ISceneNode.Draw(Render.RenderContext context)
        {
            Renderable.DrawEx(context, worldTransformation);
        }

        void Render.ISceneNode.Visit(Action<Render.ISceneNode> callback) { callback(this); }

        void Render.ISceneNode.CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            Renderable.CalculateLocalMouse(mouseRay, debug, worldTransformation);
        }
    }
}
