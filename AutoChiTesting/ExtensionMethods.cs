using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChiTesting
{
    public static class ExtensionMethods
    {
        public static double[] Flatten(this double[,,] array)
        {
            return array.Cast<double>().ToArray();
        }
    }
}
