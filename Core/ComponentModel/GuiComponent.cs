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

namespace Gem.ComponentModel
{
    public class GuiComponent : Component
    {
        public GuiModule module = null;
        public Gem.Gui.Renderable renderable = null;
    }
}
