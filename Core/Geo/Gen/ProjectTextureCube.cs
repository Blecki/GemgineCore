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
        public struct TexturePlane
        {
            public Vector3 origin;
            public Vector3 uAxis;
            public Vector3 vAxis;
            public Vector3 normal;
        }

        public static void ProjectTexturePlanes(Mesh m, TexturePlane[] planes)
        {
            for (int i = 0; i < m.VertexCount; ++i)
            {
                var closest = -1;
                var angle = 1.0f;
                for (int p = 0; p < planes.Length; ++p)
                {
                    var pangle = Gem.Math.Vector.AngleBetweenVectors(m.verticies[i].Normal, planes[p].normal);
                    if (pangle > angle)
                    {
                        angle = pangle;
                        closest = p;
                    }
                }

                if (closest >= 0)
                    m.verticies[i].TextureCoordinate = ProjectTexture(m.verticies[i].Position,
                        planes[closest].origin, planes[closest].uAxis, planes[closest].vAxis);
            }
        }

        private static TexturePlane[] cubePlanes;

        public static void ProjectTextureCube(Mesh m)
        {
            if (cubePlanes == null)
            {
                cubePlanes = new TexturePlane[6];

                cubePlanes[0] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(0, 0, -1),
                    uAxis = new Vector3(1, 0, 0),
                    vAxis = new Vector3(0, 1, 0)
                };

                cubePlanes[1] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(0, 0, 1),
                    uAxis = new Vector3(1, 0, 0),
                    vAxis = new Vector3(0, 1, 0)
                };

                cubePlanes[2] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(-1, 0, 0),
                    uAxis = new Vector3(0, 0, 1),
                    vAxis = new Vector3(0, 1, 0)
                };

                cubePlanes[3] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(1, 0, 0),
                    uAxis = new Vector3(0, 0, 1),
                    vAxis = new Vector3(0, 1, 0)
                };

                cubePlanes[4] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(0, -1, 0),
                    uAxis = new Vector3(0, 0, 1),
                    vAxis = new Vector3(1, 0, 0)
                };

                cubePlanes[5] = new TexturePlane
                {
                    origin = Vector3.Zero,
                    normal = new Vector3(0, 1, 0),
                    uAxis = new Vector3(0, 0, 1),
                    vAxis = new Vector3(1, 0, 0)
                };
            }

            ProjectTexturePlanes(m, cubePlanes);
        }
    }
}