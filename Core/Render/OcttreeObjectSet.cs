using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Render
{
    public class OcttreeObjectSet : ObjectSet
    {
        public OctTreeModule octTreeModule;

        List<uint> GetVisibleObjects(BoundingFrustum camera)
        {
            return new List<uint>(octTreeModule.Query(camera).Distinct());
        }
    }
}
