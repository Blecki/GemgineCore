using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem
{
    public class ConsoleWindow
    {
        public Console.VirtualConsole ScriptConsole;
        public Action<String> ConsoleCommandHandler = null;
        public RenderTarget2D ConsoleRenderSurface;
        public Rectangle ScreenPosition;
        Main Owner;
        GraphicsDevice Graphics;
        IServiceProvider Services;

        public ConsoleWindow(
            Main Owner, 
            GraphicsDevice graphics,
            IServiceProvider services,
            Rectangle ScreenPosition,
            int width,
            int height)
        {
            this.Owner = Owner;
            this.Graphics = graphics;
            this.Services = services;
            Resize(ScreenPosition, width, height);
        }

        public void Resize(Rectangle ScreenPosition, int width, int height)
        {
            this.ScreenPosition = ScreenPosition;

            ScriptConsole = new Console.VirtualConsole(command =>
            {
                Owner.InjectAction(() =>
                {
                    try
                    {
                        if (this.ConsoleCommandHandler != null) ConsoleCommandHandler(command);
                    }
                    catch (Exception e)
                    {
                        ScriptConsole.Write(e.Message);
                    }
                });
            },
          Graphics, new ContentManager(Services, "Content"), width, height);

            ConsoleRenderSurface = new RenderTarget2D(Graphics, ScreenPosition.Width, ScreenPosition.Height);
        }
    }
}
