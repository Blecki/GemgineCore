using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gem.Console
{
    public class VirtualConsole
    {
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;

        TextDisplay display;
        DynamicConsoleBuffer dynamicConsole;
        List<String> commandRecallBuffer = new List<String>();
        int recallBufferPlace = 0;
        System.Threading.Mutex handlerMutex = new System.Threading.Mutex();
        public void Write(String s) { dynamicConsole.Write(s); }
        public void WriteLine(String s) { dynamicConsole.Write(s + "\n"); }
        public Action<string> commandHandler;
        public void Clear() { dynamicConsole.Clear(); }
        public String Title { get { return dynamicConsole.title; } set { dynamicConsole.title = value; } }
        
        
            private bool ctrlModifier = false;
            private bool shiftModifier = false;
                
        public VirtualConsole(
            Action<String> processCommand,
            GraphicsDevice graphicsDevice, 
            ContentManager contentManager,
            int width,
            int height)
        {
            commandHandler = processCommand;

            this.graphicsDevice = graphicsDevice;
            this.contentManager = contentManager;

            display = new TextDisplay(width, height, graphicsDevice, contentManager);
            dynamicConsole = new DynamicConsoleBuffer(2048, display);
        }

        public void KeyDown(System.Windows.Forms.Keys key, int keyValue)
        {
                    handlerMutex.WaitOne();

                    if (keyValue == (int)System.Windows.Forms.Keys.ControlKey)
                        ctrlModifier = true;
                    else if (keyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                        shiftModifier = true;
                    else if (key == System.Windows.Forms.Keys.Right && ctrlModifier == true)
                    {
                        if (dynamicConsole.activeInput == dynamicConsole.inputs[0])
                            dynamicConsole.activeInput = dynamicConsole.inputs[1];
                        else
                            dynamicConsole.activeInput = dynamicConsole.inputs[0];
                    }
                    else if (key == System.Windows.Forms.Keys.Up && ctrlModifier == true)
                    {
                        dynamicConsole.outputScrollPoint += 1;
                    }
                    else if (key == System.Windows.Forms.Keys.Down && ctrlModifier == true)
                    {
                        dynamicConsole.outputScrollPoint -= 1;
                        if (dynamicConsole.outputScrollPoint < 0) dynamicConsole.outputScrollPoint = 0;
                    }
                    else if (key == System.Windows.Forms.Keys.Up && shiftModifier == true)
                    {
                        if (commandRecallBuffer.Count != 0)
                        {
                            recallBufferPlace -= 1;
                            if (recallBufferPlace < 0) recallBufferPlace = commandRecallBuffer.Count - 1;
                            dynamicConsole.activeInput.cursor = 0;
                            dynamicConsole.activeInput.input = commandRecallBuffer[recallBufferPlace];
                        }
                    }
                    else if (key == System.Windows.Forms.Keys.Down && shiftModifier == true)
                    {
                        if (commandRecallBuffer.Count != 0)
                        {
                            recallBufferPlace += 1;
                            if (recallBufferPlace >= commandRecallBuffer.Count) recallBufferPlace = 0;
                            dynamicConsole.activeInput.cursor = 0;
                            dynamicConsole.activeInput.input = commandRecallBuffer[recallBufferPlace];
                        }
                    }
                    else if (key == System.Windows.Forms.Keys.Up && ctrlModifier == false)
                    {
                        dynamicConsole.activeInput.cursor -= display.width;
                        if (dynamicConsole.activeInput.cursor < 0) dynamicConsole.activeInput.cursor += display.width;
                    }
                    else if (key == System.Windows.Forms.Keys.Down && ctrlModifier == false)
                    {
                        dynamicConsole.activeInput.cursor += display.width;
                        if (dynamicConsole.activeInput.cursor > dynamicConsole.activeInput.input.Length)
                            dynamicConsole.activeInput.cursor = dynamicConsole.activeInput.input.Length;
                    }
                    else if (key == System.Windows.Forms.Keys.Left && ctrlModifier == false)
                    {
                        dynamicConsole.activeInput.cursor -= 1;
                        if (dynamicConsole.activeInput.cursor < 0) dynamicConsole.activeInput.cursor = 0;
                    }
                    else if (key == System.Windows.Forms.Keys.Right && ctrlModifier == false)
                    {
                        dynamicConsole.activeInput.cursor += 1;
                        if (dynamicConsole.activeInput.cursor > dynamicConsole.activeInput.input.Length)
                            dynamicConsole.activeInput.cursor = dynamicConsole.activeInput.input.Length;
                    }
                    else if (key == System.Windows.Forms.Keys.Delete && ctrlModifier == false)
                    {
                        var front = dynamicConsole.activeInput.cursor;
                        var sofar = dynamicConsole.activeInput.input.Substring(0, front);
                        var back = dynamicConsole.activeInput.input.Length - dynamicConsole.activeInput.cursor - 1;
                        if (back > 0) sofar += dynamicConsole.activeInput.input.Substring(dynamicConsole.activeInput.cursor + 1, back);
                        dynamicConsole.activeInput.input = sofar;
                    }

                    handlerMutex.ReleaseMutex();
        }

        public void KeyUp(System.Windows.Forms.Keys key, int keyValue)
        {
            handlerMutex.WaitOne();

            if (keyValue == (int)System.Windows.Forms.Keys.ControlKey)
                ctrlModifier = false;
            else if (keyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                shiftModifier = false;

            handlerMutex.ReleaseMutex();
        }

        public void KeyPress(char keyChar)
        {
            handlerMutex.WaitOne();

            if (ctrlModifier == true)
            {
                if (keyChar == '\n')
                {
                    dynamicConsole.Write(dynamicConsole.activeInput.input + "\n");
                    var s = dynamicConsole.activeInput.input;
                    dynamicConsole.activeInput.input = "";
                    dynamicConsole.outputScrollPoint = 0;
                    dynamicConsole.activeInput.cursor = 0;
                    dynamicConsole.activeInput.scroll = 0;
                    commandRecallBuffer.Add(s);
                    recallBufferPlace = commandRecallBuffer.Count;
                    commandHandler(s);
                }
            }
            else
            {
                if (keyChar == (char)System.Windows.Forms.Keys.Enter)
                {
                    var newPosition = (int)System.Math.Ceiling((float)(dynamicConsole.activeInput.cursor + 1) / display.width)
                        * display.width;
                    if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                        dynamicConsole.activeInput.input =
                            dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor,
                            new String(' ', newPosition - dynamicConsole.activeInput.cursor));
                    else
                        dynamicConsole.activeInput.input += new String(' ', newPosition - dynamicConsole.activeInput.cursor);
                    dynamicConsole.activeInput.cursor = newPosition;
                }
                else if (keyChar == (char)System.Windows.Forms.Keys.Tab)
                {
                    var newPosition = (int)System.Math.Ceiling((float)(dynamicConsole.activeInput.cursor + 1) / 4) * 4;
                    if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                        dynamicConsole.activeInput.input =
                            dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor,
                            new String(' ', newPosition - dynamicConsole.activeInput.cursor));
                    else
                        dynamicConsole.activeInput.input += new String(' ', newPosition - dynamicConsole.activeInput.cursor);
                    dynamicConsole.activeInput.cursor = newPosition;
                }
                else if (keyChar == (char)System.Windows.Forms.Keys.Back)
                {
                    if (dynamicConsole.activeInput.cursor > 0)
                    {
                        var front = dynamicConsole.activeInput.cursor - 1;
                        var sofar = dynamicConsole.activeInput.input.Substring(0, front);
                        var back = dynamicConsole.activeInput.input.Length - dynamicConsole.activeInput.cursor;
                        if (back > 0) sofar +=
                            dynamicConsole.activeInput.input.Substring(dynamicConsole.activeInput.cursor, back);
                        dynamicConsole.activeInput.input = sofar;
                        dynamicConsole.activeInput.cursor -= 1;
                    }
                }
                else
                {
                    if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                        dynamicConsole.activeInput.input =
                            dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor,
                            new String(keyChar, 1));
                    else
                        dynamicConsole.activeInput.input += keyChar;
                    dynamicConsole.activeInput.cursor += 1;
                }
            }

            handlerMutex.ReleaseMutex();
        }

        public void Draw(RenderTarget2D onto = null)
        {
            handlerMutex.WaitOne();

            graphicsDevice.SetRenderTarget(onto);
            dynamicConsole.PopulateDisplay();
            display.Draw(graphicsDevice);
            graphicsDevice.SetRenderTarget(null);

            handlerMutex.ReleaseMutex();
        }
    }
}
