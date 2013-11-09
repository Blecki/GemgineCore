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
        /// <summary>
        /// Scale mesh part so that it fits - and fills - the supplied bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static void FitToBox(Mesh mesh, BoundingBox box)
        {
            var partBox = Gen.CalculateBoundingBox(mesh);
            var partDims = partBox.Max - partBox.Min;
            var boxDims = box.Max - box.Min;
            Morph(mesh, (v) =>
                {
                    var rel = v - partBox.Min;
                    return box.Min + ((rel / partDims) * boxDims);
                });
        }

        public static Mesh FitToBoxCopy(Mesh mesh, BoundingBox box)
        {
            var result = Copy(mesh);
            FitToBox(result, box);
            return result;
        }
    }
}