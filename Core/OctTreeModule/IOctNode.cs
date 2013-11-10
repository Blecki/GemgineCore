using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Gem
{
    public interface IOctNode
    {
        BoundingSphere Bounds { get; }
    }
}
