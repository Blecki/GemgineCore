using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render.Cameras
{
    public class OrthographicCamera : ICamera
    {
        public Vector2 focus;
		public float zoom = 1.0f;
        public Viewport Viewport { get; set; }
        public float rotation = 0.0f;
        public Vector3 GetEyeVector() { return Vector3.UnitZ; }

        public OrthographicCamera(Viewport viewport)
        {
            this.Viewport = viewport;
            this.focus = new Vector2(viewport.Width / 2, viewport.Height / 2);
        }

        public Vector3 GetPosition() { return new Vector3(focus, -1); }

        public void Yaw(float f)
        {
        }

        public void Pitch(float f)
        {
        }

        public void Roll(float f)
        {
            rotation += 5;
        }

        public void Pan(float X, float Y, float speed)
        {
            focus.X += X * speed;
            focus.Y += Y * speed;
        }

        public void Zoom(float d)
        {
            zoom += d;
        }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreateOrthographicOffCenter(-Viewport.Width / 2, Viewport.Width / 2,
                    Viewport.Height / 2, -Viewport.Height / 2, -32, 32);
            }
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateTranslation(-focus.X, -focus.Y, 0.0f) 
                    * Matrix.CreateRotationZ(rotation) 
                    * Matrix.CreateScale(zoom);
            }
        }

        public Matrix World
        {
            get
            {
                return Matrix.Identity;
            }
        }

        /// <summary>
        /// Transforms a position from screen space to world space.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 Unproject(Vector3 vec)
        {
            return Viewport.Unproject(vec, Projection, View, World);
        }

        /// <summary>
        /// Transforms a position from world space to screen space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 Project(Vector3 vec)
        {
            return Viewport.Project(vec, Projection, View, World);
        }

        public Matrix GetSinglePixelProjection(Vector2 Pixel)
        {
            var NP0 = Viewport.Unproject(new Vector3(Pixel, 0), Projection, Matrix.Identity, Matrix.Identity);
            var NP1 = Viewport.Unproject(new Vector3(Pixel + Vector2.One, 0), Projection, Matrix.Identity, Matrix.Identity);
            var Min = new Vector2(System.Math.Min(NP0.X, NP1.X), System.Math.Min(NP0.Y, NP1.Y));
            var Max = new Vector2(System.Math.Max(NP0.X, NP1.X), System.Math.Max(NP0.Y, NP1.Y));
            return Matrix.CreateOrthographicOffCenter(Min.X, Max.X, Min.Y, Max.Y, -32, 32);
        }

        public BoundingFrustum GetFrustum()
        {
            return new BoundingFrustum(View * Projection);
        }
    }

}
