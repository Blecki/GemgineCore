using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class BranchNode : ISceneNode
    {
        internal BranchNode parent = null;

        public Euler Orientation = null;

        public BranchNode(Euler Orientation = null)
        {
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        private List<ISceneNode> children = new List<ISceneNode>();

        public void Add(ISceneNode child) { children.Add(child); }
        public void Remove(ISceneNode child) { children.Remove(child); }
        public IEnumerator<ISceneNode> GetEnumerator() { return children.GetEnumerator(); }

        public void UpdateWorldTransform(Matrix m)
        {
            foreach (var child in this)
                child.UpdateWorldTransform(m * Orientation.Transform);
        }

        public virtual void Draw(RenderContext context)
        {
            foreach (var child in this)
                child.Draw(context);
        }

        public void Visit(Action<Render.ISceneNode> callback)
        {
            callback(this);
            foreach (var child in this)
                child.Visit(callback);
        }

        public void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            foreach (var child in this)
                child.CalculateLocalMouse(mouseRay, debug);
        }

    }
}
