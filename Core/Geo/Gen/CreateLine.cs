using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        public static Mesh CreateLine(Vector3 v0, Vector3 v1, Vector3 normal, float width)
        {
            var lineV = v1 - v0;
            lineV.Normalize();
            var lineP = Vector3.Cross(lineV, normal);

            var result = new Mesh();
            result.verticies = new VertexPositionNormalTexture[4];
            result.verticies[0].Position = v0 + (lineP * (width / 2.0f));
            result.verticies[1].Position = v0 - (lineP * (width / 2.0f));
            result.verticies[2].Position = v1 - (lineP * (width / 2.0f));
            result.verticies[3].Position = v1 + (lineP * (width / 2.0f));

            result.indicies = new short[] { 0, 1, 2, 3, 0, 2 };
            return result;
        }
    }
}