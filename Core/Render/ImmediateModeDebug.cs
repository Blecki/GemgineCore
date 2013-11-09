using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Gem.Render
{
    public class ImmediateModeDebug
    {
        GraphicsDevice GraphicsDevice;
        BasicEffect Effect;

        const int MaxImmediateModeVerticies = 2048;
        VertexPositionColor[] immediateModeVerticies = new VertexPositionColor[MaxImmediateModeVerticies];
        int immediateModeVertexCount = 0;
        VertexPositionColor[] tempVerticies = new VertexPositionColor[4];

        enum ActivePrimitive
        {
            Triangles,
            Lines
        }

        ActivePrimitive activePrimitive = ActivePrimitive.Lines;

        public void Begin(Matrix world, Matrix view, Matrix projection)
        {
            Effect.World = world;
            Effect.View = view;
            Effect.Projection = projection;
        }

        public void Triangle(VertexPositionColor A, VertexPositionColor B, VertexPositionColor C)
        {
            if (activePrimitive == ActivePrimitive.Lines && immediateModeVertexCount > 0) Flush();
            activePrimitive = ActivePrimitive.Triangles;
            if (immediateModeVertexCount + 3 > MaxImmediateModeVerticies) Flush();

            immediateModeVerticies[immediateModeVertexCount] = A;
            immediateModeVerticies[immediateModeVertexCount + 1] = B;
            immediateModeVerticies[immediateModeVertexCount + 2] = C;
            immediateModeVertexCount += 3;
        }

        public void Line(VertexPositionColor A, VertexPositionColor B)
        {
            if (activePrimitive == ActivePrimitive.Triangles && immediateModeVertexCount > 0) Flush();
            activePrimitive = ActivePrimitive.Lines;
            if (immediateModeVertexCount + 2 > MaxImmediateModeVerticies) Flush();

            immediateModeVerticies[immediateModeVertexCount] = A;
            immediateModeVerticies[immediateModeVertexCount + 1] = B;
            immediateModeVertexCount += 2;
        }

        public void BoundingBox(BoundingBox box, Color c)
        {
            //Bottom loop
            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), c));
 
            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), c));
            
            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), c));
            
            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), c));

            //Top loop
            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), c),
                new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), c),
                new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), c));

            //Vertical
            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), c),
              new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), c));

            Line(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), c),
                new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), c));

            
        }

        public void Flush()
        {
            if (immediateModeVertexCount > 0)
            {
                Effect.CurrentTechnique.Passes[0].Apply();
                if (activePrimitive == ActivePrimitive.Lines)
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, immediateModeVerticies, 0, immediateModeVertexCount / 2);
                else
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, immediateModeVerticies, 0, immediateModeVertexCount / 3);
            }
            immediateModeVertexCount = 0;
        }

        public ImmediateModeDebug(GraphicsDevice GraphicsDevice)
        {
            this.GraphicsDevice = GraphicsDevice;
            this.Effect = new BasicEffect(GraphicsDevice);
            Effect.TextureEnabled = false;
            Effect.VertexColorEnabled = true;
        }

        
    }
}
