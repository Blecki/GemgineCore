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
        public static Mesh CreateTexturedFacetedCube()
        {
            var result = CreateCube();
            result = FacetCopy(result);
            ProjectTextureCube(result);

            for (int i = 0; i < result.VertexCount; ++i)
                result.verticies[i].TextureCoordinate += new Vector2(0.5f, 0.5f);

            return result;
        }
    }
}