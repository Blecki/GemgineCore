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
        private Texture2D fontData;
        public float glypthWidth;
        public float glypthHeight;
        public float kerningWidth;

        public BitmapFont(Texture2D font, float gWidth, float gHeight, float kWidth)
        {
            fontData = font;
            glypthWidth = gWidth;
            glypthHeight = gHeight;
            kerningWidth = kWidth;
        }

        public static void RenderText(
            String text, float X, float Y, float wrapAt, Render.ImmediateMode2d context, BitmapFont font, float depth = 0)
        {
            context.Texture = font.fontData;

            var x = X;
            var y = Y;

            var kx = (font.glypthWidth - font.kerningWidth) / 2;
            int col = (int)font.fontData.Width / (int)font.glypthWidth;

            for (var i = 0; i < text.Length; ++i)
            {
                if (x >= wrapAt)
                {
                    y += font.glypthHeight;
                    x = X;
                }

                var code = text[i];
                if (code == '\n')
                {
                    x = X;
                    y += font.glypthHeight;
                }
                else if (code == ' ')
                {
                    x += font.kerningWidth;
                }
                else if (code < 0x80)
                {
                    //code -= (char)0x20;
                    float fx = (code % col) * font.glypthWidth;
                    float fy = (code / col) * font.glypthHeight;

                    context.Glyph(x, y, font.glypthWidth, font.glypthHeight, fx / font.fontData.Width,
                        fy / font.fontData.Height, font.glypthWidth / font.fontData.Width,
                        font.glypthHeight / font.fontData.Height, depth);
                    //context.Quad(X, Y, font.glypthWidth, font.glypthHeight, fx, fy, font.glypthWidth, font.glyphHeight

                    x += font.kerningWidth;
                }
            }

        }

    }

}