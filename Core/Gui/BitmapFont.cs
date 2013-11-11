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

namespace Gem.Gui
{
    public class BitmapFont
    {
        public Texture2D fontData;
        public int glyphWidth;
        public int glyphHeight;
        public int kerningWidth;

        public double fgWidth;
        public double fgHeight;

        public int Columns { get; private set; }

        public BitmapFont(Texture2D font, int gWidth, int gHeight, int kWidth)
        {
            fontData = font;
            glyphWidth = gWidth;
            glyphHeight = gHeight;
            kerningWidth = kWidth;

            Columns = (int)font.Width / glyphWidth;

            fgWidth = (double)glyphWidth / (double)font.Width;
            fgHeight = (double)glyphHeight / (double)font.Height;
        }

        public static void RenderText(
            String text, float X, float Y, float wrapAt, Render.ImmediateMode2d context, BitmapFont font, float depth = 0)
        {
            context.Texture = font.fontData;

            var x = X;
            var y = Y;

            var kx = (font.glyphWidth - font.kerningWidth) / 2;
            int col = (int)font.fontData.Width / (int)font.glyphWidth;

            for (var i = 0; i < text.Length; ++i)
            {
                if (x >= wrapAt)
                {
                    y += font.glyphHeight;
                    x = X;
                }

                var code = text[i];
                if (code == '\n')
                {
                    x = X;
                    y += font.glyphHeight;
                }
                else if (code == ' ')
                {
                    x += font.kerningWidth;
                }
                else if (code < 0x80)
                {
                    float fx = (code % col) * font.glyphWidth;
                    float fy = (code / col) * font.glyphHeight;

                    context.Glyph(x, y, font.glyphWidth, font.glyphHeight, fx / font.fontData.Width,
                        fy / font.fontData.Height, (float)font.glyphWidth / (float)font.fontData.Width,
                        (float)font.glyphHeight / (float)font.fontData.Height, depth);

                    x += font.kerningWidth;
                }
            }

        }

    }

}