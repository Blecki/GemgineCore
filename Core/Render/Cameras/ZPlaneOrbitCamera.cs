using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render.Cameras
{
    public class ZPlaneOrbitCamera : ICamera
    {
        private float max_pitch;
        private float min_pitch;
        private float _z_plane;

        public float z_plane
        {
            get
            {
                return _z_plane;
            }
            set
            {
                _z_plane = value;
                Focus.Z = _z_plane;
            }
        }
        public Viewport Viewport { get; set; }
        public float NearPlane = 0.1f;
        public float FarPlane { get; set; }
        public float Distance = 1.0f;

        public ZPlaneOrbitCamera(float max_pitch, float min_pitch, float z_plane, float distance)
        {
            this.max_pitch = max_pitch;
            this.min_pitch = min_pitch;
            this.z_plane = z_plane;
            this.Distance = distance;
            pitch = max_pitch;
            FarPlane = 1000.0f;
        }

        public Vector3 Focus = new Vector3(0, 0, 0);
        public Vector3 Position { get { return Focus; } set { Focus = value; } }
        public Vector3 GetPosition() { return Vector3.Transform(Vector3.Zero, View); }
        public Vector3 GetEyeVector() { return Focus - Position; }

        public float yaw;
        public float pitch;

        private static float NormalizeAngle(float A)
        {
            float Temp = (float)System.Math.Floor(A / (System.Math.PI * 2));
            float Temp2 = (float)(Temp * (System.Math.PI * 2));
            float Remainder = A - Temp2;
            return Remainder;
        }

        public void Yaw(float f)
        {
            yaw += f;
            yaw = NormalizeAngle(yaw);
        }

        public void Pitch(float f)
        {
            pitch += f;
            if (pitch < min_pitch) pitch = min_pitch;
            if (pitch > max_pitch) pitch = max_pitch;
        }

        public void Roll(float f) { }

        public void Pan(float X, float Y, float speed)
        {
            Vector3 camera_shift_y = new Vector3(0, speed, 0);

            camera_shift_y = Vector3.Transform(camera_shift_y,
                Matrix.CreateRotationZ(-yaw));
            Vector3 camera_shift_x = Vector3.Transform(camera_shift_y,
                Matrix.CreateRotationZ(MathHelper.ToRadians(90)));

            if (System.Math.Abs(X) > 0.1f)
                Focus -= camera_shift_x * X;
            if (System.Math.Abs(Y) > 0.1f)
                Focus += camera_shift_y * Y;

            Focus.Z = z_plane;

        }

        public void Zoom(float d) { Distance += d; }

        public Matrix View
        {
            get
            {
                Matrix m = Matrix.CreateTranslation(0, 0, -Distance);
                m = Matrix.CreateRotationX(-pitch) * m;
                m = Matrix.CreateRotationZ(yaw) * m;
                m = Matrix.CreateTranslation(-Focus) * m;

                return m;
            }
        }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspective((float)Viewport.Width / (float)Viewport.Height * NearPlane, NearPlane, NearPlane, FarPlane);
            }
        }

        public Matrix GetSinglePixelProjection(Vector2 Pixel)
        {
            var NP0 = Viewport.Unproject(new Vector3(Pixel, 0), Projection, Matrix.Identity, Matrix.Identity);
            var NP1 = Viewport.Unproject(new Vector3(Pixel + Vector2.One, 0), Projection, Matrix.Identity, Matrix.Identity);
            var Min = new Vector2(System.Math.Min(NP0.X, NP1.X), System.Math.Min(NP0.Y, NP1.Y));
            var Max = new Vector2(System.Math.Max(NP0.X, NP1.X), System.Math.Max(NP0.Y, NP1.Y));
            return Matrix.CreatePerspectiveOffCenter(Min.X, Max.X, Min.Y, Max.Y, NearPlane, FarPlane);

        }

        public Vector3 Unproject(Vector3 Pos)
        {
            return Viewport.Unproject(Pos, Projection, View, Matrix.Identity);
        }

        public Vector3 Project(Vector3 Pos)
        {
            return Viewport.Project(Pos, Projection, View, Matrix.Identity);
        }

        public BoundingFrustum GetFrustum() { return new BoundingFrustum(View * Projection); }
    }
}
