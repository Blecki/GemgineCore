using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Csg
    {
        public static RealtimeCSG.Plane[] PlanesFromMesh(Mesh mesh)
        {
            var result = new List<RealtimeCSG.Plane>();

            for (var i = 0; i < mesh.indicies.Length; i += 3)
            {
                var planeNormal = Gen.CalculateNormal(mesh, mesh.indicies[i], mesh.indicies[i + 1], mesh.indicies[i + 2]);
                var plane = new Plane(mesh.verticies[mesh.indicies[i]].Position,
                    mesh.verticies[mesh.indicies[i + 1]].Position,
                    mesh.verticies[mesh.indicies[i + 2]].Position);
                result.Add(new RealtimeCSG.Plane(new RealtimeCSG.Vector3(plane.Normal.X,
                    plane.Normal.Y, plane.Normal.Z), plane.D));
            }

            var final = new List<RealtimeCSG.Plane>();
            foreach (var plane in result)
            {
                bool reject = false;
                foreach (var existingPlane in final)
                    if (plane.EpsilonEquals(existingPlane)) reject = true;
                if (!reject) final.Add(plane);
            }

            return final.ToArray();
        }      
    }
}