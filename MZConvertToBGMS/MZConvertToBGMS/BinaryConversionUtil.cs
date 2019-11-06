using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZConvertToBGMS {
    class BinaryConversionUtil {
        public static readonly int BUFFERSIZE = 1048576;
        public static readonly int FLOATSIZE = sizeof(float);
        public static readonly int INTSIZE = sizeof(int);
        public static readonly int DOUBLESIZE = sizeof(double);
        public static readonly int BOOLSIZE = sizeof(bool);
        public static readonly int LONGSIZE = sizeof(long);
        public static readonly int ULONGSIZE = sizeof(ulong);
        public static readonly int CHARSIZE = sizeof(char);
        public static readonly int BYTESIZE = sizeof(byte);

        /// <summary>
        /// Read some bytes from a Stream
        /// </summary>
        /// <param name="stream">the stream to read from</param>
        /// <param name="buffer">the target buffer must not exist yet</param>
        /// <param name="size">the number of bytes to read from the stream</param>
        /// <returns></returns>
        public static byte[] read(Stream stream, ref byte[] buffer, int size) {
            if (buffer == null || size > buffer.Length) {
                buffer = new byte[size];
            }

            stream.Read(buffer, 0, size);
            return buffer;
        }


        public static int write(Stream stream, byte[] buffer) {
            stream.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        public static byte[] Compress(byte[] data) {
            if (data.Length > 0) {
                byte[] compressesData = null;
                using (MemoryStream outputStream = new MemoryStream()) {
                    using (GZipStream zip = new GZipStream(outputStream, CompressionMode.Compress)) {
                        zip.Write(data, 0, data.Length);
                    }
                    compressesData = outputStream.ToArray();
                }

                return compressesData;
            }
            return data;
        }

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

        public static int readInt(byte[] b) {
            int[] v = new int[1];
            System.Buffer.BlockCopy(b, 0, v, 0, INTSIZE);
            return v[0];
        }


        public static int readInt(byte[] b, int offset) {
            int[] v = new int[1];
            System.Buffer.BlockCopy(b, offset, v, 0, INTSIZE);
            return v[0];
        }

        public static bool readBool(Stream stream) {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return readBool(buffer);
        }

        public static bool readBool(byte[] b) {
            return (b != null) && (b.Length > 0) && b[0] == 1;
        }

        public static int readInt(Stream stream) {
            byte[] buffer = new byte[INTSIZE];
            stream.Read(buffer, 0, INTSIZE);
            return readInt(buffer);
        }

        public static long readLong(byte[] b) {
            long[] v = new long[1];
            System.Buffer.BlockCopy(b, 0, v, 0, LONGSIZE);
            return v[0];
        }

        public static long readLong(Stream stream) {
            byte[] buffer = new byte[LONGSIZE];
            stream.Read(buffer, 0, LONGSIZE);
            return readLong(buffer);
        }

        public static float readFloat(Stream stream) {
            byte[] buffer = new byte[FLOATSIZE];
            stream.Read(buffer, 0, FLOATSIZE);
            return readFloat(buffer);
        }

        public static byte readByte(Stream stream) {
            byte[] buffer = new byte[BYTESIZE];
            stream.Read(buffer, 0, BYTESIZE);
            return buffer[0];
        }


        public static float readFloat(byte[] b) {
            float[] v = new float[1];
            System.Buffer.BlockCopy(b, 0, v, 0, FLOATSIZE);
            return v[0];
        }

        public static double readDouble(byte[] b) {
            double[] v = new double[1];
            System.Buffer.BlockCopy(b, 0, v, 0, DOUBLESIZE);
            return v[0];
        }

        public static double readDouble(Stream stream) {
            byte[] buffer = new byte[DOUBLESIZE];
            stream.Read(buffer, 0, DOUBLESIZE);
            return readDouble(buffer);
        }

        public static ulong readULong(byte[] b) {
            ulong[] v = new ulong[1];
            System.Buffer.BlockCopy(b, 0, v, 0, ULONGSIZE);

            return v[0];
        }

        public static ulong readULong(Stream stream) {
            byte[] buffer = new byte[ULONGSIZE];
            stream.Read(buffer, 0, ULONGSIZE);
            return readULong(buffer);
        }

        public static float[,] readFloatArray2(byte[] b, int size, int dim) {
            var v = new float[dim, (size / dim) / FLOATSIZE];

            System.Buffer.BlockCopy(b, 0, v, 0, size);
            return v;
        }

        public static float[,] readFloatArray2D(byte[] b, int size) {
            var v = new float[2, (size >> 3)];
            System.Buffer.BlockCopy(b, 0, v, 0, size);
            return v;
        }

        public static double[,] readDoubleArray2(byte[] b, int size, int dim) {
            double[,] v = new double[dim, (size / dim) / DOUBLESIZE];

            System.Buffer.BlockCopy(b, 0, v, 0, size);
            return v;
        }


        public static double[,] readDoubleArray2D(byte[] b, int size) {
            var v = new double[2, (size >> 4)];
            System.Buffer.BlockCopy(b, 0, v, 0, size);
            return v;
        }

        public static float[] readFloatArray(byte[] b, int size) {
            float[] v = new float[size / FLOATSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static float[] readFloatArray(Stream stream, int size) {
            byte[] buffer = new byte[size];
            stream.Read(buffer, 0, size);
            return readFloatArray(buffer, size);
        }

        public static double[] readDoubleArray(byte[] b, int size) {
            double[] v = new double[size / DOUBLESIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static bool[] readBoolArray(byte[] b, int size) {
            bool[] v = new bool[size / BOOLSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static long[] readLongArray(byte[] b, int size) {
            long[] v = new long[size / LONGSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static ulong[] readULongArray(byte[] b, int size) {
            ulong[] v = new ulong[size / ULONGSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static int[] readIntArray(byte[] b, int size) {
            int[] v = new int[size / INTSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return v;
        }

        public static string readStringWithSizeAnnotation(Stream stream) {
            int size = readInt(stream);
            string s = readString(stream, size);
            return s;
        }

        public static string readString(Stream stream, int size) {
            byte[] buffer = new byte[size];
            stream.Read(buffer, 0, size);
            return readString(buffer, size);
        }

        public static string readString(byte[] b) {
            return readString(b, b.Length);
        }

        public static string readString(byte[] b, int size) {
            char[] v = new char[size / CHARSIZE];
            System.Buffer.BlockCopy(b, 0, v, 0, size);

            return new string(v);
        }

        public static byte[] toByte(float[,] d) {
            byte[] b = new byte[FLOATSIZE * d.Length];
            System.Buffer.BlockCopy(d, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(double[,] d) {
            byte[] b = new byte[DOUBLESIZE * d.Length];
            System.Buffer.BlockCopy(d, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(long[] l) {
            byte[] b = new byte[LONGSIZE * l.Length];
            System.Buffer.BlockCopy(l, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(int[] l) {
            byte[] b = new byte[INTSIZE * l.Length];
            System.Buffer.BlockCopy(l, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(ulong[] l) {
            byte[] b = new byte[ULONGSIZE * l.Length];
            System.Buffer.BlockCopy(l, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(float[] d) {
            byte[] b = new byte[FLOATSIZE * d.Length];
            System.Buffer.BlockCopy(d, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(double[] d) {
            byte[] b = new byte[DOUBLESIZE * d.Length];
            System.Buffer.BlockCopy(d, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(bool[] d) {
            byte[] b = new byte[BOOLSIZE * d.Length];
            System.Buffer.BlockCopy(d, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(int i) {
            byte[] b = new byte[INTSIZE];
            System.Buffer.BlockCopy(new int[] { i }, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(byte b) {
            return new byte[] { b };
        }

        public static byte[] toByte(float f) {
            byte[] b = new byte[FLOATSIZE];
            System.Buffer.BlockCopy(new float[] { f }, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(long f) {
            byte[] b = new byte[LONGSIZE];
            System.Buffer.BlockCopy(new long[] { f }, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(double f) {
            byte[] b = new byte[DOUBLESIZE];
            System.Buffer.BlockCopy(new double[] { f }, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(string s) {
            byte[] b = new byte[CHARSIZE * s.Length];
            System.Buffer.BlockCopy(s.ToCharArray(), 0, b, 0, b.Length);
            return b;
        }

        public static byte[] StringToByteWithSizeAnnotation(string s) {
            int sSize = (CHARSIZE * s.Length);
            byte[] b = new byte[sSize + sizeof(int)];
            byte[] bS = toByte(sSize);

            System.Buffer.BlockCopy(bS, 0, b, 0, bS.Length);
            System.Buffer.BlockCopy(s.ToCharArray(), 0, b, sizeof(int), sSize);
            return b;
        }

        public static byte[] toByte(ulong l) {
            byte[] b = new byte[ULONGSIZE];
            System.Buffer.BlockCopy(new ulong[] { l }, 0, b, 0, b.Length);
            return b;
        }

        public static byte[] toByte(bool bo) {
            byte[] b = new byte[1];
            b[0] = (bo) ? (byte)1 : (byte)0;
            return b;
        }
    }
}
