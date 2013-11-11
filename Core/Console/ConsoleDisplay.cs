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
    /// Renders a grid of characters.
    /// </summary>
    public class ConsoleDisplay
    {
        private class Line
        {
            internal VertexBuffer buffer;
            internal bool dirty = true;
            internal ConsoleVertex[] verts;
        }
        
        Line[] lines;
        IndexBuffer indexBuffer;
        Effect effect;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Gem.Gui.BitmapFont Font { get; private set; }

        public ConsoleDisplay(int width, int height, Gui.BitmapFont font, GraphicsDevice device, ContentManager content)
        {
            this.Width = width;
            this.Height = height;

            this.Font = font;
            effect = content.Load<Effect>("Content/draw-console");


            //Prepare the index buffer.
            var indicies = new short[width * 6];
            for (int i = 0; i < width; ++i)
            {
                indicies[i * 6 + 0] = (short)(i * 4 + 0);
                indicies[i * 6 + 1] = (short)(i * 4 + 1);
                indicies[i * 6 + 2] = (short)(i * 4 + 2);
                indicies[i * 6 + 3] = (short)(i * 4 + 2);
                indicies[i * 6 + 4] = (short)(i * 4 + 3);
                indicies[i * 6 + 5] = (short)(i * 4 + 0);
            }

            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indicies.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indicies);

            //Prepare vertex buffers
            lines = new Line[height];
        
            for (int y = 0; y < height; ++y)
            {
                lines[y] = new Line();
                lines[y].verts = new ConsoleVertex[width * 4];
                for (int i = 0; i < width; ++i)
                {
                    lines[y].verts[i * 4 + 0].Position = new Vector3(i * font.glyphWidth - 0.5f, y * font.glyphHeight - 0.5f, 0);
                    lines[y].verts[i * 4 + 1].Position = new Vector3((i + 1) * font.glyphWidth - 0.5f, y * font.glyphHeight - 0.5f, 0);
                    lines[y].verts[i * 4 + 2].Position = new Vector3((i + 1) * font.glyphWidth - 0.5f, (y + 1) * font.glyphHeight - 0.5f, 0);
                    lines[y].verts[i * 4 + 3].Position = new Vector3(i * font.glyphWidth - 0.5f, (y + 1) * font.glyphHeight - 0.5f, 0);

                    lines[y].verts[i * 4 + 0].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 1].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 2].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 3].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 0].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 1].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 2].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 3].BGColor = Color.Black.ToVector4();
                }
                lines[y].buffer = new VertexBuffer(device, typeof(ConsoleVertex), lines[y].verts.Length, BufferUsage.None);
            }
        }

        /// <summary>
        /// Set the character at a certain point in the grid.
        /// </summary>
        /// <param name="place">The point to set the character at, treating the grid like a 1-dimensional array.</param>
        /// <param name="character">The character to set. Supported characters vary by font.</param>
        /// <param name="fg">Foreground color. Null if no change to color data.</param>
        /// <param name="bg">Background color. Null if no change to color data.</param>
        public void SetChar(int place, int character, Color? fg = null, Color? bg = null)
        {
            //Find character in font
            var charX = character % Font.Columns;
            var charY = character / Font.Columns;

            //Convert place to row/column format
            var row = place / Width;
            place %= Width;

            //Map glyph to quad
            lines[row].verts[place * 4 + 0].TextureCoordinate = new Vector2(
                (float)(charX * Font.fgWidth), 
                (float)(charY * Font.fgHeight));
            lines[row].verts[place * 4 + 1].TextureCoordinate = new Vector2(
                (float)((charX + 1) * Font.fgWidth), 
                (float)(charY * Font.fgHeight));
            lines[row].verts[place * 4 + 2].TextureCoordinate = new Vector2(
                (float)((charX + 1) * Font.fgWidth),
                (float)((charY + 1) * Font.fgHeight));
            lines[row].verts[place * 4 + 3].TextureCoordinate = new Vector2(
                (float)(charX * Font.fgWidth), 
                (float)((charY + 1) * Font.fgHeight));

            if (fg != null)
            {
                lines[row].verts[place * 4 + 0].FGColor = fg.Value.ToVector4();
                lines[row].verts[place * 4 + 1].FGColor = fg.Value.ToVector4();
                lines[row].verts[place * 4 + 2].FGColor = fg.Value.ToVector4();
                lines[row].verts[place * 4 + 3].FGColor = fg.Value.ToVector4();
            }

            if (bg != null)
            {
                lines[row].verts[place * 4 + 0].BGColor = bg.Value.ToVector4();
                lines[row].verts[place * 4 + 1].BGColor = bg.Value.ToVector4();
                lines[row].verts[place * 4 + 2].BGColor = bg.Value.ToVector4();
                lines[row].verts[place * 4 + 3].BGColor = bg.Value.ToVector4();
            }


            lines[row].dirty = true;
        }

        private void drawRows(int start, int end, GraphicsDevice device)
        {
            while (start < end)
            {
                if (lines[start].dirty)
                {
                    lines[start].buffer.SetData(lines[start].verts);
                    lines[start].dirty = false;
                }
                device.SetVertexBuffer(lines[start].buffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Width * 4, 0, Width * 2);
                ++start;
            }
        }

        /// <summary>
        /// Draw the text grid. The drawn grid always fills the entire viewport.
        /// </summary>
        /// <param name="device"></param>
        public void Draw(GraphicsDevice device)
        {
            effect.Parameters["Texture"].SetValue(Font.fontData);
            effect.Parameters["Projection"].SetValue(
                Matrix.CreateOrthographicOffCenter(0, Width * Font.glyphWidth, Height * Font.glyphHeight, 0, -1, 1));
            effect.Parameters["View"].SetValue(Matrix.Identity);
            effect.Parameters["World"].SetValue(Matrix.Identity);

            effect.CurrentTechnique = effect.Techniques[0];

            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.AlphaBlend;

            device.Indices = indexBuffer;

            effect.CurrentTechnique.Passes[0].Apply();

            drawRows(0, Height, device);

            device.SetVertexBuffer(null);
        }
    }
}
