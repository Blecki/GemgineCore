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

        public List<Console.Window> Consoles = new List<Console.Window>();

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
            Consoles[0].ScriptConsole.WriteLine(e.Message);
            Consoles[0].ScriptConsole.WriteLine(e.StackTrace);
        }

        public void ReportError(String msg)
        {
            ConsoleOpen = true;
            Consoles[0].ScriptConsole.WriteLine(msg);
        }

        public void Write(String msg)
        {
            Consoles[0].ScriptConsole.Write(msg);
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

        public Console.Window AllocateConsole(Rectangle at, int width, int height)
        {
            Consoles.Add(new Console.Window(this, GraphicsDevice, Services, at, width, height));
            return Consoles[Consoles.Count - 1];
        }

        public void ClearConsoles()
        {
            Consoles.Clear();
        }

        protected override void LoadContent()
        {
            AllocateConsole(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                GraphicsDevice.Viewport.Width / 12, GraphicsDevice.Viewport.Height / 16);
            Immediate2d = new Gem.Render.ImmediateMode2d(GraphicsDevice);

            if (!String.IsNullOrEmpty(startupCommand))
            {
   //             System.Console.WriteLine(startupCommand);
  //              InjectAction(() => { ScriptEngine.ExecuteCommand(startupCommand); });
            }

            Input.textHook.KeyDown += (hook, args) =>
                {
                    if (ConsoleOpen)
                        Consoles[0].ScriptConsole.KeyDown(args.KeyCode, args.KeyValue);
                };

            Input.textHook.KeyUp += (hook, args) =>
                {
                    //if (consoleOpen)
                    Consoles[0].ScriptConsole.KeyUp(args.KeyCode, args.KeyValue);
                };

            Input.textHook.KeyPress += (hook, args) =>
                {
                    if (args.KeyChar == '~')
                        ConsoleOpen = !ConsoleOpen;
                    else if (ConsoleOpen)
                        Consoles[0].ScriptConsole.KeyPress(args.KeyChar);
                };

            //if (activeGame == null) this.Game = new EmptyGame();
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
                    consoleWindow.ScriptConsole.Draw(consoleWindow.ConsoleRenderSurface);

            if (activeGame != null) activeGame.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (ConsoleOpen)
            {
                foreach (var consoleWindow in Consoles)
                {
                    Immediate2d.Camera.focus = new Vector2(Immediate2d.Camera.Viewport.Width / 2,
                        Immediate2d.Camera.Viewport.Height / 2);
                    Immediate2d.BeginScene(null, false);
                    Immediate2d.Texture = consoleWindow.ConsoleRenderSurface;
                    Immediate2d.Alpha = 0.75f;
                    Immediate2d.Quad(consoleWindow.ScreenPosition);
                }
            }
            
            base.Draw(gameTime);
        }
    }
}
