using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem;
using Gem.Common;

namespace Gem.Render
{
    public class Renderer
    {
        public ICamera Camera = new Cameras.OrbitCamera(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, 10);
        BasicEffect drawEffect;
        BasicEffect drawIDEffect;
        AlphaTestEffect drawSpriteEffect;
        ImmediateModeDebug debug;
        GraphicsDevice device;

        RenderTarget2D mousePickTarget;
        RenderContext renderContext = new RenderContext();
        RenderContextID renderContextID = new RenderContextID();
        ImmediateMode2d immediate2d = null;

        List<Tuple<VertexPositionColor, VertexPositionColor>> debugLines = 
            new List<Tuple<VertexPositionColor, VertexPositionColor>>();

        public void AddDebugLine(VertexPositionColor a, VertexPositionColor b)
        {
            debugLines.Add(new Tuple<VertexPositionColor, VertexPositionColor>(a, b));
        }

        public Renderer(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            debug = new ImmediateModeDebug(device);

            drawEffect = new BasicEffect(device);
            drawEffect.TextureEnabled = true;
            drawEffect.VertexColorEnabled = false;

            drawSpriteEffect = new AlphaTestEffect(device);
            drawSpriteEffect.VertexColorEnabled = false;

            drawIDEffect = new BasicEffect(device);
            drawIDEffect.TextureEnabled = false;
            drawIDEffect.VertexColorEnabled = false;

            Camera.Viewport = device.Viewport;

            mousePickTarget = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Color, DepthFormat.Depth24);

            immediate2d = new ImmediateMode2d(device);
        }

        public void PreDraw(float elapsedSeconds, IRenderable renderable)
        {
            renderContext.Camera = Camera;
            renderable.PreDraw(elapsedSeconds, device, renderContext);
        }

        public Ray GetMouseRay(Vector2 mouseCoordinates)
        {
            var mouseRay = new Ray(
                Camera.GetPosition(), 
                Camera.Unproject(new Vector3(mouseCoordinates, 0)) - Camera.GetPosition());
            mouseRay.Direction = Vector3.Normalize(mouseRay.Direction);
            return mouseRay;
        }

        public IRenderable MousePick(Vector2 mouseCoordinates, IEnumerable<IRenderable> renderables)
        {
            device.SetRenderTarget(mousePickTarget);
            device.Clear(ClearOptions.Target, Vector4.Zero, 0xFFFFFF, 0);
            device.BlendState = BlendState.Opaque;

            drawIDEffect.View = Camera.View;
            drawSpriteEffect.View = Camera.View;
            var projection = Camera.GetSinglePixelProjection(mouseCoordinates);
            drawIDEffect.Projection = projection;
            drawSpriteEffect.Projection = projection;

            renderContextID.BeginScene(drawIDEffect, drawSpriteEffect, device);

            var index = 1;
            foreach (var renderable in renderables)
            {
                var idBytes = BitConverter.GetBytes(index);
                drawIDEffect.DiffuseColor = new Vector3(idBytes[0] / 255.0f, idBytes[1] / 255.0f, idBytes[2] / 255.0f);
                drawSpriteEffect.DiffuseColor = drawIDEffect.DiffuseColor;
                renderable.DrawEx(renderContextID);

                index += 1;
            }

            device.SetRenderTarget(null);
            var data = new Color[1];
            mousePickTarget.GetData(data);
            var result = data[0].PackedValue & 0x00FFFFFF; //Mask off the alpha bits.

            if (result != 0)
                return renderables.ElementAt((int)(result - 1));

            return null;
        }

        public enum DrawModeFlag
        {
            Normal,
            DebugOnly
        }

        public void Draw(IEnumerable<IRenderable> renderables, DrawModeFlag modeFlag = DrawModeFlag.Normal)
        {
            device.SetRenderTarget(null);
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;

            drawEffect.View = Camera.View;
            drawEffect.Projection = Camera.Projection;
            drawEffect.EnableDefaultLighting();

            drawSpriteEffect.View = Camera.View;
            drawSpriteEffect.Projection = Camera.Projection;

            renderContext.Camera = Camera;
            renderContext.BeginScene(drawEffect, drawSpriteEffect, device);

            if (modeFlag == DrawModeFlag.Normal)
                foreach (var node in renderables)
                    node.DrawEx(renderContext);
            
            debug.Begin(Matrix.Identity, Camera.View, Camera.Projection);
            foreach (var line in debugLines)
                debug.Line(line.Item1, line.Item2);
            debug.Flush();
            debugLines.Clear();
        }

        public RenderContext GetContext()
        {
            return renderContext;
        }
    }
}
