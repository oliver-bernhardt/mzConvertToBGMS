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

        private static string consoleClean = "\r                                                                                      \r";
        private static void processFile(string fileSource, string fileDest) {
            MSScanReader.AScanReader reader = MSScanReader.AScanReader.getScanReader(fileSource);

            if (reader != null) {
                System.IO.FileInfo info = new System.IO.FileInfo(fileSource);
                if (fileDest == null) {
                    fileDest = fileSource.Replace(info.Extension, ".bgms");
                }

                string messagePrefix = "Processing: " + info.Name+" ";
                Console.Write(messagePrefix + "...");
                string[] progress = new string[] { "...", " ..", ". .", ".. " };

                int count = 0;
                int progressCount = 0;
                bool allFine = true;

                BGSRawAPI.BGSRawFileWriter writer = new BGSRawAPI.BGSRawFileWriter(reader.getVendor().ToString(), reader.getInstrumentModel(), reader.getInstrumentSerialNumber(), reader.getAcquisitionDate(), BGSMSEnums.MSMethodType.UNKNOWN, reader.getOriginalFileName(), fileDest, false, true);

                try {
                    foreach (BGSRawAPI.ScanEvent scan in reader) {
                        writer.AddScan(scan);
                        ++count;
                        if (count % 100 == 0) {
                            Console.Write(consoleClean + messagePrefix + progress[progressCount % progress.Length]);
                            ++progressCount;
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine("Error: " + ex.Message);
                    allFine = false;
                }
                writer.Finalize();
                reader.close();
                string status = (allFine) ? "[DONE]" : "[FAILED]";

                Console.WriteLine(consoleClean + messagePrefix + status);
            }
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
