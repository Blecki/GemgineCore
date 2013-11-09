using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Math
{
    public interface Spline
    {
        Vector3 Point(float distance);
        Vector3 Tangent(float distance);
        Vector3 RotationAxis { get; }
    }
}
