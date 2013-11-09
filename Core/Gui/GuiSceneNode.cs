﻿using System;
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
    public class SceneNode : Render.ISceneNode
    {
        private GuiModule module = null;
        internal Render.Cameras.Orthographic uiCamera = null;
        public UIItem uiRoot = null;
        internal RenderTarget2D renderTarget = null;
        private CompiledModel quadModel = null;
        public Euler Orientation;

        internal bool MouseHover = false;
        internal int LocalMouseX = 0;
        internal int LocalMouseY = 0;

        public SceneNode(int width, int height, Euler Euler = null)
        {
            this.Orientation = Euler;
            if (this.Orientation == null) this.Orientation = new Euler();

            uiCamera = new Render.Cameras.Orthographic(new Viewport(0, 0, width, height));
            uiRoot = new UIItem(new Rectangle(0, 0, width, height));
            uiRoot.settings = new PropertySet();

            uiCamera.focus = new Vector2(width / 2, height / 2);
        }

        public void ClearUI() { uiRoot.children.Clear(); }

        public void Initialize(GuiModule module, GraphicsDevice device)
        {
            this.module = module;
               renderTarget = new RenderTarget2D(device, uiCamera.Viewport.Width, uiCamera.Viewport.Height);
                var rawGuiQuad = Geo.Gen.CreateQuad();
                rawGuiQuad = Geo.Gen.FacetCopy(rawGuiQuad);
                quadModel = CompiledModel.CompileModel(rawGuiQuad, device);
                uiRoot.defaults = module.defaultSettings;
        }

        public static float ScalarProjection(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(A, B) / B.Length();
        }

        public void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            MouseHover = false;

            var verts = new Vector3[3];
            verts[0] = new Vector3(-0.5f, -0.5f, 0);
            verts[1] = new Vector3(0.5f, -0.5f, 0);
            verts[2] = new Vector3(-0.5f, 0.5f, 0);

            for (int i = 0; i < 3; ++i)
                verts[i] = Vector3.Transform(verts[i], worldTransformation);

            debug(new VertexPositionColor(verts[0], Color.Red), new VertexPositionColor(verts[1], Color.Red));
            debug(new VertexPositionColor(verts[0], Color.Green), new VertexPositionColor(verts[2], Color.Green));

            var distance = mouseRay.Intersects(new Plane(verts[0], verts[1], verts[2]));
            if (distance == null || !distance.HasValue) return;
            if (distance.Value < 0) return; //GUI plane is behind camera
            var interesectionPoint = mouseRay.Position + (mouseRay.Direction * distance.Value);

            debug(new VertexPositionColor(verts[0], Color.Blue), new VertexPositionColor(interesectionPoint, Color.Blue));

            var x = ScalarProjection(interesectionPoint - verts[0], verts[1] - verts[0]) / (verts[1] - verts[0]).Length();
            var y = ScalarProjection(interesectionPoint - verts[0], verts[2] - verts[0]) / (verts[2] - verts[0]).Length();

            LocalMouseX = (int)(x * uiCamera.Viewport.Width);
            LocalMouseY = (int)(y * uiCamera.Viewport.Height);

            MouseHover = true;
        }

        private Matrix worldTransformation = Matrix.Identity;

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m * Orientation.Transform;
        }

        public virtual void Draw(Render.RenderContext context)
        {
            context.Color = Vector3.One;
            context.Texture = renderTarget;
            context.World = worldTransformation;
            context.ApplyChanges();
            context.Draw(quadModel);
        }

        public void Visit(Action<Render.ISceneNode> callback) { callback(this); }
    }
}
