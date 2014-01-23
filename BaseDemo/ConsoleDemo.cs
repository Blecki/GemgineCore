using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace BaseDemo
{
    public class ConsoleDemo : IScreen
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        public ConsoleDemo()
        {
        }

        public void Begin()
        {
            Main.ConsoleOpen = true;
            var mainConsole = Main.Consoles[0];
            mainConsole.Resize(new Rectangle(0, 0, 800, 300), 2);
            mainConsole.Title = "MAIN CONSOLE WINDOW";
            mainConsole.Info = "<-- OUTPUT INFO | INPUT INFO -->";
            mainConsole.WriteLine("Keyboard controls:");
            mainConsole.WriteLine("   CTRL+UP/DOWN:  Scroll output window");
            mainConsole.WriteLine("   SHIFT+UP/DOWN: Scroll through command history");
            mainConsole.WriteLine("   CTRL+RIGHT:    Switch input buffers");
            mainConsole.WriteLine("   ARROWS:        Navigate input");
            mainConsole.WriteLine("   CTRL+ENTER:    Send command");

            var sideConsole = Main.AllocateConsole(new Rectangle(0, 300, 400, 300));
            sideConsole.WriteLine("You can create multiple console  windows. Consoles are            automatically sized so that text renders correctly.");
            sideConsole.HideInput = true;

            var tinyConsole = Main.AllocateConsole(new Rectangle(400, 300, 400, 150), 1);
            tinyConsole.WriteLine("Font sizes can be changed, but only by whole numbers.");
            tinyConsole.WriteLine("The input box can be hidden, as seen on these lower consoles.");
            tinyConsole.HideInput = true;

            var giantConsole = Main.AllocateConsole(new Rectangle(400, 450, 400, 150), 3);
            giantConsole.Title = "CONSOLES HAVE TITLES";
            giantConsole.WriteLine("This console has a font size of 3.");
            giantConsole.HideInput = true;
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
        }

        public void Draw(float elapsedSeconds)
        {
            Main.GraphicsDevice.Clear(Color.Black);
        }
    }
}
