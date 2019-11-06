using System;
using System.Collections.Generic;
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
    }
}
