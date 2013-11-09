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
    public class GuiModule : IModule
    {
        private GraphicsDevice device = null;
        private Render.ImmediateMode2d uiRenderer = null;
        private Input Input = null;
        public PropertySet defaultSettings = null;
        private Simulation sim;
        private ComponentMapping<UInt32, SceneNode> activeGuis = new ComponentMapping<uint, SceneNode>();

        public GuiModule(GraphicsDevice device, Input input)
        {
            this.device = device;
            uiRenderer = new Render.ImmediateMode2d(device);

            defaultSettings = new PropertySet(
                "bg-color", new Vector3(0, 0, 0),
                "text-color", new Vector3(1, 1, 1),
                "fg-color", new Vector3(1, 1, 1),
                "hidden-container", null
                );

            this.Input = input;
        }

        public void DrawGuis()
        {
            foreach (var guiNode in activeGuis)
                DrawRoot(guiNode.uiRoot, guiNode.uiCamera, guiNode.renderTarget);
        }

        public void DrawRoot(UIItem root, Render.Cameras.Orthographic camera, RenderTarget2D target)
        {
            uiRenderer.Camera = camera;
            uiRenderer.BeginScene(target);
            root.Render(uiRenderer);
        }

        #region IModule members
        void IModule.BeginSimulation(Simulation sim)
        {
            defaultSettings.Upsert("font", new BitmapFont(sim.Content.Load<Texture2D>("small-font"), 16, 16, 10));
            this.sim = sim;
        }

        void IModule.EndSimulation()
        {
        }

        void IModule.AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
                if (component is Render.SceneGraphRoot)
                {
                    (component as Render.SceneGraphRoot).rootNode.Visit(node =>
                        {
                            if (node is SceneNode)
                            {
                                activeGuis.Add(component.EntityID, node as SceneNode);
                                (node as SceneNode).Initialize(this, device);
                            }
                        });
                }
            }
        }

        void IModule.RemoveEntities(List<UInt32> entities)
        {
            foreach (var id in entities) activeGuis.Remove(id);
        }

        void IModule.Update(float elapsedSeconds)
        {
            var mousePressed = Input.Check("click");

            foreach (var guiNode in activeGuis)
            {
                guiNode.uiRoot.HandleMouse(guiNode.MouseHover,
                    guiNode.LocalMouseX, guiNode.LocalMouseY, mousePressed, sim);
                guiNode.MouseHover = false;
            }
        }

        #endregion
    }
}
