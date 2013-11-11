using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public class Mesh
    {
        public VertexPositionNormalTexture[] verticies;
        public short[] indicies;
        public short[] lineIndicies;

        public int VertexCount { get { return verticies.Length; } }

        public VertexPositionNormalTexture GetVertex(int i)
        {
            return verticies[i];
        }

        public void PrepareLineIndicies()
        {
            lineIndicies = new short[indicies.Length * 2];
            for (int i = 0; i < indicies.Length; i += 3)
            {
                lineIndicies[i * 2] = indicies[i];
                lineIndicies[i * 2 + 1] = indicies[i + 1];
                lineIndicies[i * 2 + 2] = indicies[i + 1];
                lineIndicies[i * 2 + 3] = indicies[i + 2];
                lineIndicies[i * 2 + 4] = indicies[i + 2];
                lineIndicies[i * 2 + 5] = indicies[i];
            }
        }
    }
}