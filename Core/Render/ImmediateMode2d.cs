using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class ImmediateMode2d
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        private Cameras.OrthographicCamera _camera = null;
        public Cameras.OrthographicCamera Camera { get { return _camera; } set { _camera = value; stateChanges = true; } }
        public MatrixStack MatrixStack { get; private set; }
        public BasicEffect Effect { get; private set; }
        public Texture2D Black { get; private set; }
        public Texture2D White { get; private set; }

        private bool stateChanges = false;

        VertexPositionTexture[] VertexBuffer = new VertexPositionTexture[8];
        Vector2[] TempVectors = new Vector2[8];

        public ImmediateMode2d(GraphicsDevice device)
        {
            this.GraphicsDevice = device;
            //Effect = new AlphaTestEffect(device);
            Effect = new BasicEffect(device);
            Camera = new Cameras.OrthographicCamera(device.Viewport);
            MatrixStack = new MatrixStack();

            Black = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Black.SetData(new Color[] { new Color(0, 0, 0, 255) });

            White = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            White.SetData(new Color[] { new Color(255, 255, 255, 255) });
            //Effect.ReferenceAlpha = 32;
            Effect.TextureEnabled = true;
        }

        public void PushMatrix()
        {
            MatrixStack.PushMatrix(Matrix.Identity);
        }

        public void Identity()
        {
            MatrixStack.ReplaceTop(Matrix.Identity);
            stateChanges = true;
        }

        public void PopMatrix()
        {
            MatrixStack.PopMatrix();
            stateChanges = true;
        }

        public void Transform(Matrix t)
        {
            MatrixStack.ReplaceTop(t * MatrixStack.TopMatrix );//* t);
            stateChanges = true;
        }

        public Texture2D Texture
        {
            get { return Effect.Texture; }
            set { Effect.Texture = value; stateChanges = true; }
        }

        public Vector3 Color
        {
            get { return Effect.DiffuseColor; }
            set { Effect.DiffuseColor = value; stateChanges = true; }
        }

        public float Alpha
        {
            get { return Effect.Alpha; }
            set { Effect.Alpha = value; stateChanges = true; }
        }

        public void BeginScene(RenderTarget2D target = null, bool clear = true)
        {
            stateChanges = true;
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            if (clear) GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
        }

        public void Apply()
        {
            stateChanges = false;
            Effect.Projection = Camera.Projection;
            Effect.View = Camera.View;
            Effect.World = MatrixStack.TopMatrix;
         
            //Effect.CurrentTechnique = Effect.Techniques[0];
            Effect.CurrentTechnique.Passes[0].Apply();

        }

        public void Quad(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float depth = 0)
        {
            if (stateChanges) Apply();

            VertexBuffer[0].Position = new Vector3(v0, depth);
            VertexBuffer[1].Position = new Vector3(v1, depth);
            VertexBuffer[2].Position = new Vector3(v2, depth);
            VertexBuffer[3].Position = new Vector3(v3, depth);
            VertexBuffer[0].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[1].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Glyph(float x, float y, float w, float h, float tx, float ty, float tw, float th, float depth = 0)
        {
            if (stateChanges) Apply();

            VertexBuffer[0].Position = new Vector3(x, y, depth);
            VertexBuffer[1].Position = new Vector3(x + w, y, depth);
            VertexBuffer[2].Position = new Vector3(x, y + h, depth);
            VertexBuffer[3].Position = new Vector3(x + w, y + h, depth);
            VertexBuffer[0].TextureCoordinate = new Vector2(tx, ty);
            VertexBuffer[1].TextureCoordinate = new Vector2(tx + tw, ty);
            VertexBuffer[2].TextureCoordinate = new Vector2(tx, ty + th);
            VertexBuffer[3].TextureCoordinate = new Vector2(tx + tw, ty + th);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Quad(float x, float y, float w, float h, float depth = 0)
        {
            Quad(new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x, y + h),
                new Vector2(x + w, y + h), depth);
        }

        public void Quad(Rectangle rect, float depth = 0)
        {
            Quad(new Vector2(rect.X - 0.5f, rect.Y - 0.5f),
               new Vector2(rect.X + rect.Width - 0.5f, rect.Y - 0.5f),
               new Vector2(rect.X - 0.5f, rect.Y + rect.Height - 0.5f),
               new Vector2(rect.X - 0.5f + rect.Width, rect.Y + rect.Height - 0.5f), depth);
        }

        public void Quad(Vector2[] verts, Vector2[] texcoords, float z = 0.0f)
        {
            if (stateChanges) Apply();

            for (int i = 0; i < 4; ++i)
            {
                VertexBuffer[i].Position = new Vector3(verts[i], z);
                VertexBuffer[i].TextureCoordinate = texcoords[i];
            }

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void RawFullScreenQuad()
        {
            if (stateChanges) Apply();

            VertexBuffer[0].Position = new Vector3(-1, -1, 0);
            VertexBuffer[1].Position = new Vector3(-1, 1, 0);
            VertexBuffer[3].Position = new Vector3(1, 1, 0);
            VertexBuffer[2].Position = new Vector3(1, -1, 0);

            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void OrientedSprite(Vector2 Orientation)
        {
            if (stateChanges) Apply();

            VertexBuffer[2].Position = new Vector3(Orientation.X - Orientation.Y, Orientation.X + Orientation.Y, 0.0f);
            VertexBuffer[3].Position = new Vector3(-(Orientation.X + Orientation.Y), Orientation.X - Orientation.Y, 0.0f);
            VertexBuffer[1].Position = new Vector3(Orientation.Y - Orientation.X, -(Orientation.X + Orientation.Y), 0.0f);
            VertexBuffer[0].Position = new Vector3(Orientation.X + Orientation.Y, Orientation.Y - Orientation.X, 0.0f);

            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void SpriteAt(Vector2 Position, Vector2 Orientation, float Z)
        {
            if (stateChanges) Apply();

            VertexBuffer[2].Position = new Vector3(Position.X + Orientation.X - Orientation.Y, Position.Y + Orientation.X + Orientation.Y, Z);
            VertexBuffer[3].Position = new Vector3(Position.X + -(Orientation.X + Orientation.Y), Position.Y + Orientation.X - Orientation.Y, Z);
            VertexBuffer[1].Position = new Vector3(Position.X + Orientation.Y - Orientation.X, Position.Y + -(Orientation.X + Orientation.Y), Z);
            VertexBuffer[0].Position = new Vector3(Position.X + Orientation.X + Orientation.Y, Position.Y + Orientation.Y - Orientation.X, Z);

            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Sprite(bool Flip)
        {
            if (stateChanges) Apply();

            VertexBuffer[0].Position = new Vector3(-0.5f, -0.5f, 0);
            VertexBuffer[1].Position = new Vector3(-0.5f, 0.5f, 0);
            VertexBuffer[2].Position = new Vector3(0.5f, -0.5f, 0);
            VertexBuffer[3].Position = new Vector3(0.5f, 0.5f, 0);

            if (!Flip)
            {
                VertexBuffer[0].TextureCoordinate = new Vector2(0, 0);
                VertexBuffer[1].TextureCoordinate = new Vector2(0, 1);
                VertexBuffer[2].TextureCoordinate = new Vector2(1, 0);
                VertexBuffer[3].TextureCoordinate = new Vector2(1, 1);

            }
            else
            {
                VertexBuffer[0].TextureCoordinate = new Vector2(1, 1);
                VertexBuffer[1].TextureCoordinate = new Vector2(1, 0);
                VertexBuffer[3].TextureCoordinate = new Vector2(0, 0);
                VertexBuffer[2].TextureCoordinate = new Vector2(0, 1);
            }

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Box(Vector2 Position, Vector2 Scale, float Angle, float Width)
        {
            if (stateChanges) Apply();

            TempVectors[0] = new Vector2(-0.5f, -0.5f);
            TempVectors[1] = new Vector2(-0.5f, 0.5f);
            TempVectors[2] = new Vector2(0.5f, 0.5f);
            TempVectors[3] = new Vector2(0.5f, -0.5f);

            DrawLine(TempVectors[0], TempVectors[1], Width);
            DrawLine(TempVectors[1], TempVectors[2], Width);
            DrawLine(TempVectors[2], TempVectors[3], Width);
            DrawLine(TempVectors[3], TempVectors[0], Width);
        }

        

        public void DrawLine(Vector2 Start, Vector2 End, float Width)
        {
            if (stateChanges) Apply();

            var LineNormal = Vector2.Normalize(End - Start);
            LineNormal = new Vector2(LineNormal.Y, -LineNormal.X);
            LineNormal *= Width * 0.5f;

            VertexBuffer[0].Position = new Vector3(Start + LineNormal, 0);
            VertexBuffer[1].Position = new Vector3(Start - LineNormal, 0);
            VertexBuffer[3].Position = new Vector3(End - LineNormal, 0);
            VertexBuffer[2].Position = new Vector3(End + LineNormal, 0);
            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }
    }
}
