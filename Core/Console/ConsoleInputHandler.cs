using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gem.Console
{
    /// <summary>
    /// Translates keypresses into operations on a textBufferBuffer
    /// </summary>
    public class ConsoleInputHandler
    {
        DynamicConsoleBuffer textBuffer;
        List<String> commandRecallBuffer = new List<String>();
        int recallBufferPlace = 0;
        bool ctrlModifier = false;
        bool shiftModifier = false;

        public Action<string> OnCommand;
        public int displayWidth { get; private set; }

        /// <summary>
        /// Create a console input handler
        /// </summary>
        /// <param name="OnCommand">Call back to invoke when ctrl+enter is pressed</param>
        /// <param name="Buffer">Buffer this handler will manipulate</param>
        /// <param name="DisplayWidth">The width of the underlying display, in characters</param>
        public ConsoleInputHandler(
            Action<String> OnCommand,
            DynamicConsoleBuffer Buffer,
            int DisplayWidth)
        {
            this.displayWidth = DisplayWidth;
            this.OnCommand = OnCommand;
            this.textBuffer = Buffer;
        }

        public void KeyDown(System.Windows.Forms.Keys key, int keyValue)
        {
            if (keyValue == (int)System.Windows.Forms.Keys.ControlKey)
                ctrlModifier = true;
            else if (keyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                shiftModifier = true;
            else if (key == System.Windows.Forms.Keys.Right && ctrlModifier == true) 
            {
                //Ctrl + right switches inputs
                if (textBuffer.activeInput == textBuffer.inputs[0])
                    textBuffer.activeInput = textBuffer.inputs[1];
                else
                    textBuffer.activeInput = textBuffer.inputs[0];
            }
            else if (key == System.Windows.Forms.Keys.Up && ctrlModifier == true)
            {
                //Ctrl + up scrolls the output window
                textBuffer.outputScrollPoint += 1;
            }
            else if (key == System.Windows.Forms.Keys.Down && ctrlModifier == true)
            {
                //Ctrl + down scrolls the output window
                textBuffer.outputScrollPoint -= 1;
                if (textBuffer.outputScrollPoint < 0) textBuffer.outputScrollPoint = 0;
            }
            else if (key == System.Windows.Forms.Keys.Up && shiftModifier == true)
            {
                //Shift + up recalls the previous command
                if (commandRecallBuffer.Count != 0)
                {
                    recallBufferPlace -= 1;
                    if (recallBufferPlace < 0) recallBufferPlace = commandRecallBuffer.Count - 1;
                    textBuffer.activeInput.cursor = 0;
                    textBuffer.activeInput.input = commandRecallBuffer[recallBufferPlace];
                }
            }
            else if (key == System.Windows.Forms.Keys.Down && shiftModifier == true)
            {
                //Shift + down recalls the next command, if there is one.
                if (commandRecallBuffer.Count != 0)
                {
                    recallBufferPlace += 1;
                    if (recallBufferPlace >= commandRecallBuffer.Count) recallBufferPlace = 0;
                    textBuffer.activeInput.cursor = 0;
                    textBuffer.activeInput.input = commandRecallBuffer[recallBufferPlace];
                }
            }
            else if (key == System.Windows.Forms.Keys.Up && ctrlModifier == false)
            {
                //Arrows move in the input
                textBuffer.activeInput.cursor -= displayWidth;
                if (textBuffer.activeInput.cursor < 0) textBuffer.activeInput.cursor += displayWidth;
            }
            else if (key == System.Windows.Forms.Keys.Down && ctrlModifier == false)
            {
                textBuffer.activeInput.cursor += displayWidth;
                if (textBuffer.activeInput.cursor > textBuffer.activeInput.input.Length)
                    textBuffer.activeInput.cursor = textBuffer.activeInput.input.Length;
            }
            else if (key == System.Windows.Forms.Keys.Left && ctrlModifier == false)
            {
                textBuffer.activeInput.cursor -= 1;
                if (textBuffer.activeInput.cursor < 0) textBuffer.activeInput.cursor = 0;
            }
            else if (key == System.Windows.Forms.Keys.Right && ctrlModifier == false)
            {
                textBuffer.activeInput.cursor += 1;
                if (textBuffer.activeInput.cursor > textBuffer.activeInput.input.Length)
                    textBuffer.activeInput.cursor = textBuffer.activeInput.input.Length;
            }
            else if (key == System.Windows.Forms.Keys.Delete && ctrlModifier == false)
            {
                //Delete
                var front = textBuffer.activeInput.cursor;
                var sofar = textBuffer.activeInput.input.Substring(0, front);
                var back = textBuffer.activeInput.input.Length - textBuffer.activeInput.cursor - 1;
                if (back > 0) sofar += textBuffer.activeInput.input.Substring(textBuffer.activeInput.cursor + 1, back);
                textBuffer.activeInput.input = sofar;
            }

        }

        public void KeyUp(System.Windows.Forms.Keys key, int keyValue)
        {
            //Just need to update ctrl/shift flags
            if (keyValue == (int)System.Windows.Forms.Keys.ControlKey)
                ctrlModifier = false;
            else if (keyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                shiftModifier = false;
        }

        public void KeyPress(char keyChar)
        {
            if (ctrlModifier == true)
            {
                if (keyChar == '\n')
                {
                    //Store input in recall buffer, clear input, and invoke command handler.
                    textBuffer.Write(textBuffer.activeInput.input + "\n");
                    var s = textBuffer.activeInput.input;
                    textBuffer.activeInput.input = "";
                    textBuffer.outputScrollPoint = 0;
                    textBuffer.activeInput.cursor = 0;
                    textBuffer.activeInput.scroll = 0;
                    commandRecallBuffer.Add(s);
                    recallBufferPlace = commandRecallBuffer.Count;
                    OnCommand(s);
                }
            }
            else
            {
                if (keyChar == (char)System.Windows.Forms.Keys.Enter)
                {
                    //Newline. Hack, since underlying display doesn't handle layout.
                    var newPosition = (int)System.Math.Ceiling((float)(textBuffer.activeInput.cursor + 1) / displayWidth)
                        * displayWidth;
                    if (textBuffer.activeInput.cursor < textBuffer.activeInput.input.Length)
                        textBuffer.activeInput.input =
                            textBuffer.activeInput.input.Insert(textBuffer.activeInput.cursor,
                            new String(' ', newPosition - textBuffer.activeInput.cursor));
                    else
                        textBuffer.activeInput.input += new String(' ', newPosition - textBuffer.activeInput.cursor);
                    textBuffer.activeInput.cursor = newPosition;
                }
                else if (keyChar == (char)System.Windows.Forms.Keys.Tab)
                {
                    //Tab. Convertted to spaces immediately.
                    var newPosition = (int)System.Math.Ceiling((float)(textBuffer.activeInput.cursor + 1) / 4) * 4;
                    if (textBuffer.activeInput.cursor < textBuffer.activeInput.input.Length)
                        textBuffer.activeInput.input =
                            textBuffer.activeInput.input.Insert(textBuffer.activeInput.cursor,
                            new String(' ', newPosition - textBuffer.activeInput.cursor));
                    else
                        textBuffer.activeInput.input += new String(' ', newPosition - textBuffer.activeInput.cursor);
                    textBuffer.activeInput.cursor = newPosition;
                }
                else if (keyChar == (char)System.Windows.Forms.Keys.Back)
                {
                    //Backspace.
                    if (textBuffer.activeInput.cursor > 0)
                    {
                        var front = textBuffer.activeInput.cursor - 1;
                        var sofar = textBuffer.activeInput.input.Substring(0, front);
                        var back = textBuffer.activeInput.input.Length - textBuffer.activeInput.cursor;
                        if (back > 0) sofar +=
                            textBuffer.activeInput.input.Substring(textBuffer.activeInput.cursor, back);
                        textBuffer.activeInput.input = sofar;
                        textBuffer.activeInput.cursor -= 1;
                    }
                }
                else
                {
                    //If we made it here, the key must be an ordinary key.
                    if (textBuffer.activeInput.cursor < textBuffer.activeInput.input.Length)
                        textBuffer.activeInput.input =
                            textBuffer.activeInput.input.Insert(textBuffer.activeInput.cursor,
                            new String(keyChar, 1));
                    else
                        textBuffer.activeInput.input += keyChar;
                    textBuffer.activeInput.cursor += 1;
                }
            }

        }

    }
}
