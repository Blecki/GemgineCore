using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public interface ICamera
    {
        Viewport Viewport { get; set; }
        Matrix Projection { get; }
        Matrix View { get; }

        Vector3 Unproject(Vector3 Pos);
        Vector3 Project(Vector3 vec);
        BoundingFrustum GetFrustum();
        Matrix GetSinglePixelProjection(Vector2 Pixel);

        void Yaw(float f);
        void Pitch(float f);
        void Roll(float f);
        void Pan(float X, float Y, float speed);
        void Zoom(float d);

        Vector3 GetPosition();
        Vector3 GetEyeVector();
    }
}
