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
    public class Main : Microsoft.Xna.Framework.Game
    {
        private IGame activeGame = null;
        private IGame nextGame = null;
        public IGame Game { get { return activeGame; } set { nextGame = value; } }

        public List<Console.ConsoleWindow> Consoles = new List<Console.ConsoleWindow>();

        GraphicsDeviceManager graphics;

        public Input Input { get; private set; }

        private Common.BufferedList<Action> injectedActionQueue = new Common.BufferedList<Action>();
        private System.Threading.Mutex injectedActionQueueLock = new System.Threading.Mutex();
        public Render.ImmediateMode2d Immediate2d = null;

        public bool ConsoleOpen = false;
        private string startupCommand;

        public void ReportException(Exception e)
        {
            ConsoleOpen = true;
            Consoles[0].WriteLine(e.Message);
            Consoles[0].WriteLine(e.StackTrace);
        }

        public void ReportError(String msg)
        {
            ConsoleOpen = true;
            Consoles[0].WriteLine(msg);
        }

        public void Write(String msg)
        {
            Consoles[0].Write(msg);
        }

        public Main(String startupCommand)
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Input = new Input(Window.Handle);

            this.startupCommand = startupCommand;
        }

        public Console.ConsoleWindow AllocateConsole(Rectangle at)
        {
            Consoles.Add(new Console.ConsoleWindow(GraphicsDevice, Content, at));
            return Consoles[Consoles.Count - 1];
        }

        public void ClearConsoles()
        {
            Consoles.Clear();
        }

        protected override void LoadContent()
        {
            var mainConsole = AllocateConsole(new Rectangle(0, 0, 800, 600));
            Immediate2d = new Gem.Render.ImmediateMode2d(GraphicsDevice);

            mainConsole.BindKeyboard(() => ConsoleOpen, Input);

            Input.textHook.KeyPress += (hook, args) =>
                {
                    if (args.KeyChar == '~')
                        ConsoleOpen = !ConsoleOpen;
                };
        }

        protected override void UnloadContent()
        {
            if (activeGame != null)
                activeGame.End();
            activeGame = null;
        }

        public void InjectAction(Action e)
        {
            injectedActionQueueLock.WaitOne();
            injectedActionQueue.Add(e);
            injectedActionQueueLock.ReleaseMutex();
        }

        private int ticks = 0;
        protected override void Update(GameTime gameTime)
        {
            if (ticks != 0)
            {
                if (nextGame != null)
                {
                    var saveActive = activeGame;
                    if (activeGame != null) activeGame.End();
                    activeGame = nextGame;
                    activeGame.Main = this;
                    activeGame.Input = Input;
                    try
                    {
                        activeGame.Begin();
                    }
                    catch (Exception e)
                    {
                        activeGame = saveActive;
                        if (activeGame != null) activeGame.Begin();
                        ReportException(e);
                    }
                    nextGame = null;
                }

                try
                {
                    Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (activeGame != null) activeGame.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                    injectedActionQueueLock.WaitOne();
                    injectedActionQueue.Swap();
                    foreach (var action in injectedActionQueue) action();
                    injectedActionQueue.ClearFront();
                    injectedActionQueueLock.ReleaseMutex();
                }
                catch (Exception e)
                {
                    ReportException(e);
                }
            }
            else
                ticks = 1;
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (ConsoleOpen)
                foreach (var consoleWindow in Consoles)
                    consoleWindow.PrepareImage();

            if (activeGame != null) activeGame.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (ConsoleOpen)
            {
                Immediate2d.Camera.focus = new Vector2(Immediate2d.Camera.Viewport.Width / 2,
                    Immediate2d.Camera.Viewport.Height / 2);
                Immediate2d.BeginScene(null, false);

                foreach (var consoleWindow in Consoles)
                    consoleWindow.Draw(Immediate2d);
            }
            
            base.Draw(gameTime);
        }
    }
}
