using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoChiTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            (double[,,], double[,,]) inputs = AutoChiInputs.GetInputArrays();
            (int, int, int) patchSize = AutoChiInputs.GetPatch();
            (int, int, int) stride = AutoChiInputs.GetStride();

            double[,,] autochi = AutoChiExecutor.RunAutochi(inputs.Item1, inputs.Item2, patchSize, stride);

            for (int i = 0; i < inputs.Item1.GetLength(0); i++)
            {
                for (int j = 0; j < inputs.Item1.GetLength(1); j++)
                {
                    for (int k = 0; k < inputs.Item1.GetLength(2); k++)
                    {
                        Console.Write(autochi[i, j, k]);
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            Console.Read();
        }

        //static void Main(string[] args)
        //{
        //    //double[] A_patch = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 14, 0, 56, 62, 0, 0, 0, 0, 392, 398, 0, 440, 446 };
        //    //double[] B_patch = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 5, 0, 19, 21, 0, 0, 0, 0, 131, 133, 0, 147, 149 };

        //    double[] A_patch = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 14, 20, 56, 62, 68, 0, 0, 0, 392, 398, 404, 440, 446, 452 };
        //    double[] B_patch = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 5, 7, 19, 21, 23, 0, 0, 0, 131, 133, 135, 147, 149, 151 };

        //    double autochi = GetCMean(A_patch, B_patch);

        //    Console.Write(autochi);
        //    Console.Read();
        //}
        //}

    }
}
