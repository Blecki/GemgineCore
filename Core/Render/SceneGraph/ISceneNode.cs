using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render.SceneGraph
{
    public interface ISceneNode
    {
        void UpdateWorldTransform(Matrix m);
        void Draw(RenderContext context);
        void Visit(Action<ISceneNode> callback);
        void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug);
    }
}
