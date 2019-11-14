/*
 * Original author: Oliver M. Bernhardt
 * Email: oliver.bernhardt@biognosys.com,
 *
 * Copyright (c) 2019 Oliver Bernhardt
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZConvertToBGMS {
    public class Util {
        public static float[,] convertDoublesToFloats(double[,] input) {
            if (input == null) {
                return null;
            }
            float[,] output = new float[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); ++i) {
                for (int j = 0; j < input.GetLength(1); ++j) {
                    output[i, j] = (float)input[i, j];
                }
            }
            return output;
        }

        /// <summary>
        /// Decompresses binary data that was compressed using the GZIP algorithm
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] DecompressGZIP(byte[] zippedData) {

            if (zippedData.Length > 0) {
                byte[] decompressedData = null;
                using (MemoryStream outputStream = new MemoryStream()) {
                    using (MemoryStream inputStream = new MemoryStream(zippedData)) {
                        using (GZipStream zip = new GZipStream(inputStream, CompressionMode.Decompress)) {
                            zip.CopyTo(outputStream);
                        }
                    }
                    decompressedData = outputStream.ToArray();
                }

                return decompressedData;
            }
            return zippedData;
        }

        /// <summary>
        /// Decompresses binary data that was compressed using the ZLib algorithm
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] DecompressZLib(byte[] zippedData) {

            if (zippedData.Length > 0) {
                byte[] decompressedData = null;
                using (MemoryStream outputStream = new MemoryStream()) {
                    using (MemoryStream inputStream = new MemoryStream(zippedData)) {
                        using (Ionic.Zlib.ZlibStream zip = new Ionic.Zlib.ZlibStream(inputStream, Ionic.Zlib.CompressionMode.Decompress)) {
                            zip.CopyTo(outputStream);
                        }
                    }
                    decompressedData = outputStream.ToArray();
                }

                return decompressedData;
            }
            return zippedData;
        }

        /// <summary>
        /// Takes byte[] and converts them into double[] using memory copy
        /// </summary>
        /// <param name="b"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double[] readDoubleArray(byte[] b) {            
            double[] v = new double[b.Length / sizeof(double)];
            System.Buffer.BlockCopy(b, 0, v, 0, b.Length);

            return v;
        }

        /// <summary>
        /// Takes byte[] and converts them into float[] using memory copy
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float[] readFloatArray(byte[] b) {
            float[] v = new float[b.Length / sizeof(float)];
            System.Buffer.BlockCopy(b, 0, v, 0, b.Length);

            return v;
        }


        /// <summary>
        /// Converts the M/Z - Intensity pairs from the mzXML file into the scan format that the BGMS file expects
        /// The input data is expected to be alternating m/z and corresponding intensity values
        /// data[0] -> mz
        /// data[1] -> Intensity
        /// data[2] -> mz
        /// data[3] -> Intenstiy
        /// etc...
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[,] mzPairsToScan(float[] data) {
            return mzPairsToScan(new FloatArrayWrapper(data));
        }


        /// <summary>
        /// Converts the M/Z - Intensity pairs from the mzXML file into the scan format that the BGMS file expects
        /// The input data is expected to be alternating m/z and corresponding intensity values
        /// data[0] -> mz
        /// data[1] -> Intensity
        /// data[2] -> mz
        /// data[3] -> Intenstiy
        /// etc...
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[,] mzPairsToScan(double[] data) {
            return mzPairsToScan(new DoubleArrayWrapper(data));
        }


        /// <summary>
        /// Converts the M/Z - Intensity pairs from the mzXML file into the scan format that the BGMS file expects
        /// The input data is expected to be alternating m/z and corresponding intensity values
        /// data[0] -> mz
        /// data[1] -> Intensity
        /// data[2] -> mz
        /// data[3] -> Intenstiy
        /// etc...
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[,] mzPairsToScan(IFloatArray data) {
            List<Tuple<float, float>> pairs = new List<Tuple<float, float>>(data.Length / 2);
            bool isOrdered = true;
            float lastMZ = 0.0f;

            for (int i = 0; i < data.Length; i += 2) {
                float mz = data[i];
                float intensity = data[i + 1];
                if (mz > 0.0 || intensity > 0.0) {
                    pairs.Add(new Tuple<float, float>(mz, intensity));
                    isOrdered = isOrdered && mz > lastMZ;
                    lastMZ = mz;
                }
            }

            if (!isOrdered) {
                pairs.Sort(MZPairItemOrder.INSTANCE);
            }
            float[,] scan = new float[2, pairs.Count];
            for (int i = 0; i < pairs.Count; ++i) {
                scan[0, i] = pairs[i].Item1;
                scan[1, i] = pairs[i].Item2;
            }

            return scan;
        }
    }


    public class MZPairItemOrder : IComparer<Tuple<float, float>> {
        public static MZPairItemOrder INSTANCE = new MZPairItemOrder();
        public int Compare(Tuple<float, float> x, Tuple<float, float> y) {
            return x.Item1.CompareTo(y.Item1);
        }
    }

    /// <summary>
    /// Unified wrapper around an array so that I can use the same algorithm for both double[] and float[] arrays without code duplication
    /// </summary>
    public interface IFloatArray {
        float this[int index] {
            get;
        }

        int Length {
            get;
        }
    }

    /// <summary>
    /// Wrapper around a double[] so to be used like a float[]
    /// Used to allow of unified use of one algorithm for both float[] and double[]
    /// </summary>
    public class DoubleArrayWrapper : IFloatArray {
        private double[] values;
        public DoubleArrayWrapper(double[] values) {
            this.values = values;
        }
        public float this[int index] {
            get { return (float)this.values[index]; }
        }

        public int Length { get { return this.values.Length; } }
    }

    /// <summary>
    /// Wrapper around a float[] to be used like float[]
    /// Used to allow of unified use of one algorithm for both float[] and double[]
    /// </summary>
    public class FloatArrayWrapper : IFloatArray {
        private float[] values;
        public FloatArrayWrapper(float[] values) {
            this.values = values;
        }
        public float this[int index] {
            get { return (float)this.values[index]; }
        }

        public int Length { get { return this.values.Length; } }
    }
}
