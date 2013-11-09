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
        public static Mesh CreatePrism(int sides, Vector3 radial, Vector3 axis, float height)
        {
            var basePolygon = CreateUnitPolygon(sides, radial, axis);
            var topPolygon = TransformCopy(basePolygon, Matrix.CreateTranslation(axis * height));
            basePolygon.indicies = basePolygon.indicies.Reverse().ToArray();

            var result = Merge(basePolygon, topPolygon);

            var wallIndicies = new List<short>();

            for (short i = 0; i < sides; ++i)
            {
                short start = i;
                short end = (short)(i + 1);
                if (end >= sides) end = 0;

                start += 1;
                end += 1;

                wallIndicies.AddMany(start, (short)(end + sides + 1), end);
                wallIndicies.AddMany(start, (short)(start + sides + 1), (short)(end + sides + 1));
            }

            wallIndicies.AddRange(result.indicies);
            result.indicies = wallIndicies.ToArray();

            return result;
        }
    }
}