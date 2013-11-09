using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Math
{
    public class Angle
    {
        public static float PI2 { get { return (float)System.Math.PI * 2; } }
        public static float PI { get { return (float)System.Math.PI; } }

        public static float Normalize(float A)
        {
            float Temp = (float)System.Math.Floor(A / PI2);
            float Temp2 = Temp * PI2;
            float Remainder = A - Temp2;
            return Remainder;
        }

        public static float Delta(float A, float B)
        {
            if (B > A)
            {
                float D = B - A;
                if (D > PI)
                    return -( A + ( PI2 - B ) );
                else
                    return D;
            }
            else
            {
                float D = -(A - B);
                if (D < -PI)
                    return B + ( PI2 - A );
                else
                    return D;
            }
        }
    }
}
