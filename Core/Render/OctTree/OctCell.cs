using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class OctCell
    {
        public BoundingBox Bounds;
        public OctCell[] Children;
        public List<IOctNode> Contents;

        public bool Leaf { get { return Children == null; } }

        public OctCell(BoundingBox Bounds) { this.Bounds = Bounds; }
    }
}
