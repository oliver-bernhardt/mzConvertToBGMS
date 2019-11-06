using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZConvertToBGMS {
    public class Program {

        private static readonly string IN_ARG = "-in";
        private static readonly string OUT_ARG = "-out";

        public static void Main(string[] args) {
            Dictionary<string, string> arguments = readArguments(args);

            string source;
            if (arguments.TryGetValue(IN_ARG, out source)) {
                string dest = null;
                arguments.TryGetValue(OUT_ARG, out dest);

                if (System.IO.File.Exists(source)) {
                    processFile(source, dest);
                } else if (System.IO.Directory.Exists(source)) {
                    processFolder(source, dest);
                }
            }
        }

        private static void processFolder(string source, string dest) {
            if (dest == null) {
                dest = source;
            }
            foreach (string file in System.IO.Directory.GetFiles(source)) {
                System.IO.FileInfo info = new System.IO.FileInfo(file);
                string fileDest = System.IO.Path.Combine(dest, info.Name.Replace(info.Extension, ".bgms"));
                processFile(file, fileDest);
            }
        }

        private static void processFile(string source, string dest) {
            ConversionProcessor processor = new ConversionProcessor(source, dest);
            processor.tryConvert();
        }



        private static Dictionary<string, string> readArguments(string[] args) {
            Dictionary<string, string> argsDic = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i += 2) {
                argsDic[args[i]] = args[i + 1];
            }
            return argsDic;
        }
    }
}
