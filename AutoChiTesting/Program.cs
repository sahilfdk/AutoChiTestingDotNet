using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;

namespace AutochiTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Define your 3D A and B arrays, patchSize, and stride here
            double[,,] A = new double[8, 8, 8];
            double[,,] B = new double[8, 8, 8];
            (int, int, int) patchSize = (3, 3, 3);
            (int, int, int) stride = (1, 1, 1);

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

            double[,,] autochi = RunAutochi(A, B, patchSize, stride);

            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jMax; j++)
                {
                    for (int k = 0; k < kMax; k++)
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

        public static double[,,] RunAutochi(double[,,] A, double[,,] B, (int, int, int) patchSize, (int, int, int) stride)
        {
            if (A.GetLength(0) != B.GetLength(0) || A.GetLength(1) != B.GetLength(1) || A.GetLength(2) != B.GetLength(2))
                throw new ArgumentException("A and B arrays must have the same dimensions.");

            double[,,] autochiOut = new double[A.GetLength(0), A.GetLength(1), A.GetLength(2)];
            for (int i = 0; i < autochiOut.GetLength(0); i++)
            {
                for (int j = 0; j < autochiOut.GetLength(1); j++)
                {
                    for (int k = 0; k < autochiOut.GetLength(2); k++)
                    {
                        autochiOut[i, j, k] = double.NaN;
                    }
                }
            }

            int n = A.Rank; //Rank is 1 for 1D array, 2 for 2D array and 3 for 3D array

            (int nNodes, int[] nodesDir) = GetNodes(A, stride);
            var seeds = GetSeeds(stride, nNodes, nodesDir);

            var pads = GetPads(A, patchSize, stride);

            if (n == 3)
            {
                double[,,] A_padded = PadArray(A, pads);
                double[,,] B_padded = PadArray(B, pads);

                int c = 0;
                //Parallel.For(0, A_padded.GetLength(0) - patchSize.Item1 + 1, i =>
                //{
                //    for (int j = 0; j < A_padded.GetLength(1) - patchSize.Item2 + 1; j++)
                //    {
                //        for (int k = 0; k < A_padded.GetLength(2) - patchSize.Item3 + 1; k++)
                //        {
                //            double[] A_patch = GetSubArray(A_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3).Cast<double>().ToArray();


                //            double[] B_patch = GetSubArray(B_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3).Cast<double>().ToArray();


                //            autochiOut[seeds[c].Item1, seeds[c].Item2, seeds[c].Item3] = GetCMean(A_patch, B_patch);
                //            c++;
                //        }
                //    }
                //});
                for (int i = 0; i < A_padded.GetLength(0) - patchSize.Item1 + 1; i++)
                {
                    for (int j = 0; j < A_padded.GetLength(1) - patchSize.Item2 + 1; j++)
                    {
                        for (int k = 0; k < A_padded.GetLength(2) - patchSize.Item3 + 1; k++)
                        {
                            double[] A_patch = GetSubArray(A_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3).Cast<double>().ToArray();


                            double[] B_patch = GetSubArray(B_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3).Cast<double>().ToArray();


                            autochiOut[seeds[c].Item1, seeds[c].Item2, seeds[c].Item3] = GetCMean(A_patch, B_patch);
                            c++;
                        }
                    }
                }
            }

            return autochiOut;
        }

        public static T[,,] GetSubArray<T>(T[,,] source, int start1, int end1, int start2, int end2, int start3, int end3)
        {
            int length1 = end1 - start1;
            int length2 = end2 - start2;
            int length3 = end3 - start3;

            T[,,] result = new T[length1, length2, length3];

            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    for (int k = 0; k < length3; k++)
                    {
                        result[i, j, k] = source[start1 + i, start2 + j, start3 + k];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrIn"></param>
        /// <param name="stride"></param>
        /// <returns>Tuple of Number of nodes to iterate and an Integer array which holds number in each direction</returns>
        public static (int, int[]) GetNodes(double[,,] arrIn, (int, int, int) stride)
        {
            int[] nodesDir = new int[] { (int)Math.Ceiling(arrIn.GetLength(0) / (double)stride.Item1), (int)Math.Ceiling(arrIn.GetLength(1) / (double)stride.Item2), (int)Math.Ceiling(arrIn.GetLength(2) / (double)stride.Item3) };
            int nNodes = nodesDir[0] * nodesDir[1] * nodesDir[2];
            return (nNodes, nodesDir);
        }

        //public static (int, int, int)[] GetSeeds((int, int, int) stride, int nNodes, int[] nodesDir)
        //{
        //    if (nNodes != nodesDir[0] * nodesDir[1] * nodesDir[2])
        //        throw new ArgumentException("Invalid number of nodes.");

        //    int n = 3;
        //    var seeds = new (int, int, int)[nNodes];
        //    int c = 0;

        //    for (int i = 0; i < nodesDir[0]; i++)
        //    {
        //        for (int j = 0; j < nodesDir[1]; j++)
        //        {
        //            for (int k = 0; k < nodesDir[2]; k++)
        //            {
        //                seeds[c] = (i * stride.Item1, j * stride.Item2, k * stride.Item3);
        //                c++;
        //            }
        //        }
        //    }

        //    return seeds;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stride">Tuple with strides in all 3 directions</param>
        /// <param name="nNodes">Number of total nodes</param>
        /// <param name="nodesDir">Array with elements representing the nodes in each direction</param>
        /// <returns>List of Seed points in all directions. Example for 3D 0 0 0, 0 0 1, 0 1 0, 0 1 1, 1 0 0, 1 0 1, 1 1 0, 1 1 1</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<(int, int, int)> GetSeeds((int, int, int) stride, int nNodes, int[] nodesDir)
        {
            if (nNodes != nodesDir[0] * nodesDir[1] * nodesDir[2])
                throw new ArgumentException("Invalid number of nodes.");

            int n = 3;//Represents No. of dimensions of an array
            List<(int, int, int)> seeds = new List<(int, int, int)>();

            if (n == 1)
            {
                for (int i = 0; i < nodesDir[0]; i++)
                {
                    seeds.Add((i * stride.Item1, 0, 0));
                }
            }

            if (n == 2)
            {
                for (int i = 0; i < nodesDir[0]; i++)
                {
                    for (int j = 0; j < nodesDir[1]; j++)
                    {
                        seeds.Add((i * stride.Item1, j * stride.Item2, 0));
                    }
                }
            }

            else if (n == 3)
            {
                for (int i = 0; i < nodesDir[0]; i++)
                {
                    for (int j = 0; j < nodesDir[1]; j++)
                    {
                        for (int k = 0; k < nodesDir[2]; k++)
                        {
                            seeds.Add((i * stride.Item1, j * stride.Item2, k * stride.Item3));
                        }
                    }
                }
            }

            return seeds;
        }


        public static ((int, int, int), (int, int, int))[] GetPads(double[,,] arrIn, (int, int, int) patchSize, (int, int, int) stride)
        {
            var pads = new ((int, int, int), (int, int, int))[3];

            for (int i = 0; i < 3; i++)
            {
                int x = patchSize.Item1 / 2;
                int y = ((arrIn.GetLength(i) - 1) % stride.Item1 <= x) ? (int)(Math.Floor((double)x) - ((arrIn.GetLength(i) - 1) % stride.Item1)) : 0;
                pads[i] = ((x, x, x), (y, 0, 0));
            }

            return pads;
        }

        public static double GetCMean(double[] A, double[] B)
        {
            if (A.Length != B.Length)
            {
                throw new ArgumentException("A and B arrays must have the same length.");
            }

            double sct = 0.0;
            double sst = 0.0;

            for (int i = 0; i < A.Length; i++)
            {
                double thetaA = Math.Atan2(Math.Abs(B[i]), -Math.Abs(A[i]));
                double r = Math.Sqrt(A[i] * A[i] + B[i] * B[i]);

                sct += Math.Cos(thetaA) * r * r;
                sst += Math.Sin(thetaA) * r * r;
            }

            double thetaCmean = Math.Atan(sst / sct);

            return MathHelper.RadiansToDegrees(thetaCmean) + 90.0;
        }


        public static double[,,] PadArray(double[,,] array, ((int, int, int), (int, int, int))[] pads)
        {
            int[] dimensions = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
            {
                dimensions[i] = array.GetLength(i);
            }

            int[] newDimensions = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
            {
                newDimensions[i] = dimensions[i] + pads[i].Item1.Item1 + pads[i].Item1.Item2;
            }

            double[,,] paddedArray = new double[newDimensions[0], newDimensions[1], newDimensions[2]];

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        paddedArray[i + pads[0].Item1.Item1, j + pads[1].Item1.Item1, k + pads[2].Item1.Item1] = array[i, j, k];
                    }
                }
            }

            return paddedArray;
        }




        public static double[,,] GetSubMatrix(double[,,] matrix, int rowStart, int rowEnd, int colStart, int colEnd, int depthStart, int depthEnd)
        {
            int numRows = rowEnd - rowStart;
            int numCols = colEnd - colStart;
            int numDepth = depthEnd - depthStart;
            double[,,] subMatrix = new double[numRows, numCols, numDepth];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    for (int k = 0; k < numDepth; k++)
                    {
                        subMatrix[i, j, k] = matrix[i + rowStart, j + colStart, k + depthStart];
                    }
                }
            }

            return subMatrix;
        }
    }

    public static class MathHelper
    {
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }
    }
}
