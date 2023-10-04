using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChiTesting
{
    internal static class MathHelper
    {
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0f / (double)Math.PI);
        }
    }
}
