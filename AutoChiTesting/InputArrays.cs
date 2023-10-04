using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChiTesting
{
    /// <summary>
    /// Define your 3D A and B arrays, patchSize, and stride here
    /// </summary>
    internal class AutoChiInputs
    {
        internal static (double[,,], double[,,]) GetInputArrays()
        {
            double[,,] A = new double[8, 8, 8];
            double[,,] B = new double[8, 8, 8];

            double a_counter = 8;
            double a_increment = 6;
            double b_counter = 3;
            double b_increment = 2;

            int iMax = A.GetUpperBound(0) + 1;
            int jMax = A.GetUpperBound(1) + 1;
            int kMax = A.GetUpperBound(2) + 1;

            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jMax; j++)
                {
                    for (int k = 0; k < kMax; k++)
                    {
                        A[i, j, k] = a_counter;
                        a_counter += a_increment;
                        B[i, j, k] = b_counter;
                        b_counter += b_increment;
                    }
                }
            }

            return (A, B);
        }

        internal static (int, int, int) GetStride()
        {
            return (1, 1, 1);
        }

        internal static (int, int, int) GetPatch()
        {
            return (3, 3, 3);
        }
    }
}
