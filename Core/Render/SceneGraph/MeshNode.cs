using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Common;
using Gem.Geo;

namespace Gem.Render.SceneGraph
{
    public class MeshNode : ISceneNode
    {
        public Euler Orientation = null;

        public Mesh Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;

        public MeshNode(Mesh mesh, Euler Orientation = null) 
        { 
            this.Mesh = mesh;
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        private Matrix worldTransformation = Matrix.Identity;

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m * Orientation.Transform;
        }

        public virtual void Draw(RenderContext context)
        {
            context.Color = Color;
            if (Texture != null) context.Texture = Texture;
            else context.Texture = context.White;
            context.World = worldTransformation;
            context.ApplyChanges();
            context.Draw(Mesh);

            if (Mesh.lineIndicies != null)
            {
                context.Color = Vector3.Zero;
                context.ApplyChanges();
                context.DrawLines(Mesh);
            }
            
        }

        public void Visit(Action<ISceneNode> callback) { callback(this); }

        public void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug) { }

    }
}
