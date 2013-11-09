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
        private Input Input = null;
        public PropertySet defaultSettings = null;
        private ComponentMapping<UInt32, Renderable> activeGuis = new ComponentMapping<uint, Renderable>();
        public Render.ImmediateMode2d GetRenderContext() { return uiRenderer; }

        public GuiDriver(GraphicsDevice device, Input input, Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            this.device = device;
            uiRenderer = new Render.ImmediateMode2d(device);

            defaultSettings = new PropertySet(
                "bg-color", new Vector3(0, 1, 0),
                "text-color", new Vector3(1, 1, 1),
                "fg-color", new Vector3(1, 1, 1),
                "hidden-container", null,
                "font", new BitmapFont(Content.Load<Texture2D>("Content/small-font"), 16, 16, 10)
                );

            this.Input = input;
        }

        public void DrawRoot(UIItem root, Render.Cameras.Orthographic camera, RenderTarget2D target)
        {
            uiRenderer.Camera = camera;
            uiRenderer.BeginScene(target);
            root.Render(uiRenderer);
        }

        public Renderable MakeGUI(UInt32 ID, int w, int h)
        {
            var r = new Renderable(device, this, w, h, null);
            activeGuis.Add(ID, r);
            return r;
        }

        public void RemoveEntities(List<UInt32> entities)
        {
            foreach (var id in entities) activeGuis.Remove(id);
        }

        public List<ObjectList> Update(float elapsedSeconds)
        {
            var mousePressed = Input.Check("click");
            var events = new List<ObjectList>();

            foreach (var guiNode in activeGuis)
            {
                guiNode.uiRoot.HandleMouseEx(guiNode.MouseHover, guiNode.LocalMouseX, guiNode.LocalMouseY, mousePressed,
                    (olist) => { events.Add(olist); });
                guiNode.MouseHover = false;
            }

            return events;
        }

        public List<ObjectList> FlatUpdate(float elapsedSeconds)
        {
            var mouse = Input.QueryAxis("primary");
            foreach (var node in activeGuis)
            {
                node.MouseHover = true;
                node.LocalMouseX = (int)(mouse.X - node.uiRoot.rect.X);
                node.LocalMouseY = (int)(mouse.Y - node.uiRoot.rect.Y);
            }
            return Update(elapsedSeconds);
        }

    }
}
