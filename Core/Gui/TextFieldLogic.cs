using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public static class TextFieldLogic
    {
        public static String HandleKeyPress(String InputBuffer, char Code)
        {
            if (Code == '\b')
            {
                if (InputBuffer.Length > 0)
                    InputBuffer = InputBuffer.Substring(0, InputBuffer.Length - 1);
            }
            else
            {
                InputBuffer += Code;
            }
            return InputBuffer;
        }


    }
}
