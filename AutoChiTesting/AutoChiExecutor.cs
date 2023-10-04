using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoChiTesting
{
    internal class AutoChiExecutor
    {
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

                Parallel.For(0, nNodes, c =>
                {
                    int i = c / (nodesDir[1] * nodesDir[2]);
                    int j = (c / nodesDir[2]) % nodesDir[1];
                    int k = c % nodesDir[2];

                    double[] A_patch = GetSubMatrix(A_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3);
                    double[] B_patch = GetSubMatrix(B_padded, i, i + patchSize.Item1, j, j + patchSize.Item2, k, k + patchSize.Item3);

                    autochiOut[seeds[c].Item1, seeds[c].Item2, seeds[c].Item3] = Math.Round(GetCMean(A_patch, B_patch), 8);
                });


                // Interpolate the missing values in autochiOut using Spline Interpolation
                if (stride.Item1 * stride.Item2 * stride.Item3 != 1)
                {
                    for (int i = 0; i < autochiOut.GetLength(2); i++)
                    {
                        if (!IsColumnNaN(autochiOut, i))
                        {
                            var pointsX = new List<double>();
                            var pointsY = new List<double>();
                            var values = new List<double>();
                            var xi = new List<double[]>();

                            for (int x = 0; x < autochiOut.GetLength(0); x++)
                            {
                                for (int y = 0; y < autochiOut.GetLength(1); y++)
                                {
                                    if (!double.IsNaN(autochiOut[x, y, i]))
                                    {
                                        pointsX.Add(x);
                                        pointsY.Add(y);
                                        values.Add(autochiOut[x, y, i]);
                                    }
                                    else
                                    {
                                        xi.Add(new double[] { x, y });
                                    }
                                }
                            }

                            // Use Spline Interpolation from Math.NET Numerics
                            var interpolatorX = CubicSpline.InterpolateNatural(pointsX, values);
                            var interpolatorY = CubicSpline.InterpolateNatural(pointsY, values);

                            for (int j = 0; j < xi.Count; j++)
                            {
                                autochiOut[(int)xi[j][0], (int)xi[j][1], i] = interpolatorX.Interpolate(xi[j][0]) * interpolatorY.Interpolate(xi[j][1]);
                            }
                        }
                    }
                }
            }

            return autochiOut;
        }
        static bool IsColumnNaN(double[,,] array, int column)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (!double.IsNaN(array[x, y, column]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrIn"></param>
        /// <param name="stride"></param>
        /// <returns>Tuple of Number of nodes to iterate and an Integer array which holds number in each direction</returns>
        public static (int, int[]) GetNodes(double[,,] arrIn, (int, int, int) stride)
        {
            int[] nodesDir = new int[] { (int)Math.Ceiling((double)arrIn.GetLength(0) / stride.Item1), (int)Math.Ceiling((double)arrIn.GetLength(1) / stride.Item2), (int)Math.Ceiling((double)arrIn.GetLength(2) / stride.Item3) };
            int nNodes = nodesDir[0] * nodesDir[1] * nodesDir[2];
            return (nNodes, nodesDir);
        }

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

            double sct = 0.0f;
            double sst = 0.0f;

            for (int i = 0; i < A.Length; i++)
            {
                double thetaA = Math.Atan2(Math.Abs(B[i]), -Math.Abs(A[i]));
                double r = Math.Sqrt(A[i] * A[i] + B[i] * B[i]);

                sct += Math.Cos(thetaA) * r * r;
                sst += Math.Sin(thetaA) * r * r;
            }

            double thetaCmean = Math.Atan(sst / sct);

            return MathHelper.RadiansToDegrees(thetaCmean) + 90.0f;
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

        static double[] GetSubMatrix(double[,,] matrix, int rowStart, int rowEnd, int colStart, int colEnd, int depthStart, int depthEnd)
        {
            int numRows = rowEnd - rowStart;
            int numCols = colEnd - colStart;
            int numDepth = depthEnd - depthStart;
            double[] subMatrix = new double[numRows * numCols * numDepth];
            int counter = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    for (int k = 0; k < numDepth; k++)
                    {
                        subMatrix[counter++] = matrix[i + rowStart, j + colStart, k + depthStart];
                    }
                }
            }
            return subMatrix;
        }
    }
}
