using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Common;
using Gem.Gui;

namespace Gem.Gui
{
    public class GuiDriver
    {
        private GraphicsDevice device = null;
        private Render.ImmediateMode2d uiRenderer = null;
        public PropertySet defaultSettings = null;
        public Render.ImmediateMode2d GetRenderContext() { return uiRenderer; }

        public GuiDriver(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            this.device = device;
            uiRenderer = new Render.ImmediateMode2d(device);

            defaultSettings = new PropertySet(
                "bg-color", new Vector3(0, 1, 0),
                "text-color", new Vector3(1, 1, 1),
                "fg-color", new Vector3(1, 1, 1),
                "hidden-container", null,
                "font", new BitmapFont(Content.Load<Texture2D>("Content/small-font"), 6, 8, 6)
                );
        }

        public void DrawRoot(UIItem root, Render.Cameras.OrthographicCamera camera, RenderTarget2D target)
        {
            uiRenderer.Camera = camera;
            uiRenderer.BeginScene(target);
            root.Render(uiRenderer);
        }

        public void DrawRenderable(Renderable renderable)
        {
            uiRenderer.Camera = renderable.uiCamera;
            uiRenderer.BeginScene(renderable.renderTarget);
            renderable.uiRoot.Render(uiRenderer);
        }

        public Renderable MakeGUI(UInt32 ID, int w, int h)
        {
            return new Renderable(device, this, w, h, null);
        }

        public List<ObjectList> Update(float elapsedSeconds, Input Input, Renderable guiNode)
        {
            var mousePressed = Input.Check("click");
            var events = new List<ObjectList>();
            guiNode.uiRoot.HandleMouseEx(guiNode.MouseHover, guiNode.LocalMouseX, guiNode.LocalMouseY, mousePressed,
               (olist) => { events.Add(olist); });
            guiNode.MouseHover = false;
            return events;
        }

        public List<ObjectList> FlatUpdate(float elapsedSeconds, Input Input, Renderable node)
        {
            var mouse = Input.QueryAxis("primary");
            node.MouseHover = true;
                node.LocalMouseX = (int)(mouse.X - node.uiRoot.rect.X);
                node.LocalMouseY = (int)(mouse.Y - node.uiRoot.rect.Y);
            return Update(elapsedSeconds, Input, node);
        }

    }
}
