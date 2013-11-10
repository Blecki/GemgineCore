using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.ComponentModel
{
    public class Component
    {
        public UInt32 EntityID { get; set; }
        public UInt32 SyncID { get; set; }
    }
}
