using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Console
{
    /// <summary>
    /// Manages the contents of a dynamic console and populates a text display.
    /// </summary>
    public class DynamicConsoleBuffer
    {
        private String output;

        public class Input
        {
            public String input { get; set; }
            public int cursor { get; set; }
            public int scroll { get; set; }

            public Input()
            {
                input = "";
                cursor = 0;
                scroll = 0;
            }
        }

        public Input activeInput { get; set; }
        public Input[] inputs;
        public int outputScrollPoint { get; set; } //Expressed in lines-from-end
        private int scrollbackSize;
        public int maxInputLines { get; set; }
        ConsoleDisplay display;

        public String title = null;
        public String info = null;
        public bool InputHidden = false;
        
        public DynamicConsoleBuffer(int scrollbackSize, ConsoleDisplay display)
        {
            this.scrollbackSize = scrollbackSize;
            Reset(display);
        }

        public void Reset(ConsoleDisplay display)
        {
            output = "";
            outputScrollPoint = 0;
            this.display = display;
            inputs = new Input[2] { new Input(), new Input() };
            activeInput = inputs[0];
            maxInputLines = 8;
        }

        public void Clear()
        {
            output = "";
            outputScrollPoint = 0;
        }

        public void rawWrite(int spot, String str, Color fg, Color bg)
        {
            for (int i = 0; i < str.Length; ++i)
                display.SetChar(spot + i, str[i], fg, bg);
        }

        public void PopulateDisplay()
        {
            var totalInputLines = (int)System.Math.Ceiling((float)(activeInput.input.Length + 1) / (float)display.Width);
            var inputLines = System.Math.Min(maxInputLines, totalInputLines);
            var visibleOutputLines = display.Height - inputLines - 1;
            if (title != null) visibleOutputLines -= 1;
            
            if (InputHidden) visibleOutputLines += inputLines + 1;
            else
            {
                if (inputLines < totalInputLines)
                {
                    //Some input is hidden. Center the scroll over it.
                    var cursorLine = (activeInput.cursor + 1) / display.Width;
                    while (cursorLine < activeInput.scroll) --activeInput.scroll;
                    while (cursorLine >= (activeInput.scroll + maxInputLines)) ++activeInput.scroll;
                }
            }

            var totalOutputLines = (int)System.Math.Ceiling((float)output.Length / (float)display.Width);
            var outputStart = totalOutputLines - visibleOutputLines - outputScrollPoint;
            if (totalOutputLines <= visibleOutputLines)
            {
                outputScrollPoint = outputStart = 0;
            }
            else if (outputStart < 0)
            {
                outputScrollPoint += outputStart;
                if (outputScrollPoint < 0) outputScrollPoint = 0;
                outputStart = 0;                
            }

            outputStart *= display.Width;

            var i = 0;
            var x = 0;
            if (!String.IsNullOrEmpty(title))
            {
                for (; i < display.Width; ++i) //Fill the title line with spaces
                    display.SetChar(i, ' ', Color.Orange, Color.Black);
                if (title.Length > display.Width) title = title.Substring(0, display.Width);
                rawWrite(((display.Width - title.Length) / 2), title, Color.Orange, Color.Black);
            }
            for (; x < display.Width * visibleOutputLines  && (x + outputStart) < output.Length; ++x, ++i)
                display.SetChar(i, output[x + outputStart], Color.White, Color.Black); //Copy output to display
            for (; x < display.Width * visibleOutputLines; ++x, ++i) //Fill rest of output area with spaces
                display.SetChar(i, ' ', Color.White, Color.Black);

            if (!InputHidden)
            {
                if (title != null) visibleOutputLines += 1;

                // Draw the info line. Output buffer info on the left, input buffer on the right, info line centered.
                var infoLine = i;
                for (; i < display.Width * (visibleOutputLines + 1); ++i) //Clear the info line
                    display.SetChar(i, ' ', Color.Orange, Color.Black);
                rawWrite(infoLine, String.Format("OH:{0,2}", outputScrollPoint), Color.Orange, Color.Black);
                var iLine = String.Format("^I:{0,2}, VI:{1,2}", activeInput.scroll, totalInputLines - maxInputLines - activeInput.scroll);
                rawWrite(infoLine + display.Width - iLine.Length, iLine, Color.Orange, Color.Black);
                if (!String.IsNullOrEmpty(info))
                {
                    if (info.Length > display.Width * 0.75) info = info.Substring(0, (int)(display.Width * 0.75));
                    rawWrite(infoLine + ((display.Width - info.Length) / 2), info, Color.Orange, Color.Black);
                }

                var inputStart = i;
                var inputc = activeInput.scroll * display.Width;
                for (; inputc < activeInput.input.Length && i < display.Height * display.Width; ++i, ++inputc)
                    display.SetChar(i, activeInput.input[inputc], Color.Red, Color.Black);
                for (; i < display.Height * display.Width; ++i)
                    display.SetChar(i, ' ', Color.Red, Color.Black);

                if (activeInput.cursor < activeInput.input.Length && activeInput.cursor >= activeInput.scroll * display.Width)
                    display.SetChar(activeInput.cursor - (activeInput.scroll * display.Width) + inputStart, activeInput.input[activeInput.cursor],
                        Color.Teal, Color.DarkGray);
                else display.SetChar(activeInput.cursor - (activeInput.scroll * display.Width) + inputStart, ' ', Color.Teal, Color.DarkGray);
            }
        }

        public void Write(String str)
        {
            foreach (var c in str)
            {
                if (c == '\n') output += new String(' ', display.Width - (output.Length % display.Width));
                else output += c;
            }

            if (output.Length > scrollbackSize * display.Width)
                output = output.Substring(output.Length - (scrollbackSize * display.Width));
        }
    }
}

