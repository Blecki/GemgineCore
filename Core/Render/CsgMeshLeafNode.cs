using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class CsgMeshLeafNode : ISceneNode
    {
        public Euler Orientation = null;

        public RealtimeCSG.CSGNode csgRoot = null;

        public CsgMeshLeafNode(RealtimeCSG.CSGNode csgRoot, Euler Orientation = null) 
        {
            this.csgRoot = csgRoot;
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        private Matrix worldTransformation = Matrix.Identity;

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m * Orientation.Transform;
        }

        private void drawCsg(RenderContext context, RealtimeCSG.CSGNode node)
        {
            if (node.NodeType == RealtimeCSG.CSGNodeType.Brush)
            {
                if (node.cachedRenderMesh == null)
                    node.cachedRenderMesh = Geo.Gen.FacetCopy(Geo.Csg.ConvertToMesh(node.cachedMesh));
                if (node.TextureProjection != null)
                    node.TextureProjection.Project(node.cachedRenderMesh, node.WorldTransformation);

                if (node.Texture == null) context.Texture = context.White;
                else context.Texture = node.Texture;
                context.Color = node.Color;
                context.ApplyChanges();
                context.Draw(node.cachedRenderMesh);
            }
            if (node.NodeType != RealtimeCSG.CSGNodeType.Brush)
            {
                drawCsg(context, node.Left);
                drawCsg(context, node.Right);
            }
        }

        public virtual void Draw(RenderContext context)
        {
            context.World = worldTransformation;
            context.Texture = context.White;
            drawCsg(context, csgRoot);            
        }

        public void Visit(Action<ISceneNode> callback) { callback(this); }

        public void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug) { }

    }
}
