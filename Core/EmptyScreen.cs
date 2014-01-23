using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gem
{
    public class EmptyScreen : IScreen
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        public EmptyScreen()
        {
        }

        public void Begin()
        {
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
        }

        public void Draw(float elapsedSeconds)
        {
          
        }
    }
}
