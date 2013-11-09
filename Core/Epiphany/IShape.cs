using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Epiphany
{
    public interface IShape
    {
        Box2D.XNA.Shape[] GetPhysicsShapes();
        Mesh GetRenderMesh();
    }
}
