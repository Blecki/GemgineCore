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

namespace Gem.Console
{
    /// <summary>
    /// Creates and encapsulates a console input handler, buffer, and display, and binds them together.
    /// </summary>
    public class ConsoleWindow
    {
        public Action<String> ConsoleCommandHandler = null;
        public Rectangle ScreenPosition { get; private set; }
        public Rectangle ActualDrawSize { get; private set; }

        public String Title
        {
            get { return Buffer.title; }
            set { Buffer.title = value; }
        }

        public String Info
        {
            get { return Buffer.info; }
            set { Buffer.info = value; }
        }

        public bool HideInput
        {
            get { return Buffer.InputHidden; }
            set { Buffer.InputHidden = value; }
        }

        GraphicsDevice Graphics;
        ContentManager Content;
        
        private Gem.Gui.BitmapFont font;
        private System.Threading.Mutex HandlerMutex = new System.Threading.Mutex();
        private Console.ConsoleInputHandler InputHandler;
        private RenderTarget2D ConsoleRenderSurface;
        private DynamicConsoleBuffer Buffer;
        private ConsoleDisplay Display;
        
        public void Write(String s) { Buffer.Write(s); }
        public void WriteLine(String s) { Buffer.Write(s + "\n"); }

        public ConsoleWindow(
            GraphicsDevice Graphics,
            ContentManager Content,
            Rectangle ScreenPosition,
            int FontScale)
        {
            this.Graphics = Graphics;
            this.Content = Content;

            font = new Gui.BitmapFont(Content.Load<Texture2D>("Content/small-font"), 6, 8, 6);

            Resize(ScreenPosition, FontScale);
        }

        public void Resize(Rectangle ScreenPosition, int FontScale)
        {
            int width = ScreenPosition.Width / (6 * FontScale);
            int height = ScreenPosition.Height / (8 * FontScale); 

            var rowSize = ScreenPosition.Height / height;
            var realHeight = height * rowSize;
            var colSize = ScreenPosition.Width / width;
            var realWidth = width * colSize;

            ActualDrawSize = new Rectangle(
                ScreenPosition.X + ((ScreenPosition.Width - realWidth) / 2),
                ScreenPosition.Y + ((ScreenPosition.Height - realHeight) / 2),
                realWidth,
                realHeight);

            Display = new ConsoleDisplay(width, height, font, Graphics, Content);
            Buffer = new DynamicConsoleBuffer(2048, Display);
            InputHandler = new ConsoleInputHandler(ConsoleCommandHandler, Buffer, width);

            this.ScreenPosition = ScreenPosition;
            ConsoleRenderSurface = new RenderTarget2D(Graphics, width * font.glyphWidth, height * font.glyphHeight, false,
                SurfaceFormat.Color, DepthFormat.Depth16);
        }

        public void PrepareImage()
        {
            HandlerMutex.WaitOne();

            Buffer.PopulateDisplay();
            Graphics.SetRenderTarget(ConsoleRenderSurface);
            Display.Draw(Graphics);
            Graphics.SetRenderTarget(null);

            HandlerMutex.ReleaseMutex();
        }

        public void Draw(Gem.Render.ImmediateMode2d Immediate2d)
        {
                    Immediate2d.Texture = ConsoleRenderSurface;
                    Immediate2d.Alpha = 0.75f;
                    Immediate2d.Quad(ActualDrawSize);
        }

        public void BindKeyboard(Func<bool> Condition, Input Input)
        {
            Input.textHook.KeyDown += (hook, args) =>
                {
                    if (Condition())
                    {
                        HandlerMutex.WaitOne();
                        this.InputHandler.KeyDown(args.KeyCode, args.KeyValue);
                        HandlerMutex.ReleaseMutex();
                    }
                };

            Input.textHook.KeyUp += (hook, args) =>
                {
                    HandlerMutex.WaitOne();
                    this.InputHandler.KeyUp(args.KeyCode, args.KeyValue);
                    HandlerMutex.ReleaseMutex();
                };

            Input.textHook.KeyPress += (hook, args) =>
                {
                    if (Condition())
                    {
                        HandlerMutex.WaitOne();
                        this.InputHandler.KeyPress(args.KeyChar);
                        HandlerMutex.ReleaseMutex();
                    }
                };
        }

    }
}
