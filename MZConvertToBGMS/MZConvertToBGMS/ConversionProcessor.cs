using MZConvertToBGMS.MSScanReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZConvertToBGMS {
    public class ConversionProcessor {
        private string destinationFile;
        private string sourceFile;
        private static string consoleClean = "\r                                                                                      \r";
        private static string[] progressIndicators = new string[] { "...", " ..", ". .", ".. " };

        public ConversionProcessor(string sourceFile, string destinationFile) {
            this.destinationFile = destinationFile;
            this.sourceFile = sourceFile;
        }

        public string tryConvert() {
            MSScanReader.AScanReader reader = MSScanReader.AScanReader.getScanReader(this.sourceFile);

            if (reader != null) {
                System.IO.FileInfo info = new System.IO.FileInfo(this.sourceFile);
                if (this.destinationFile == null) {
                    this.destinationFile = this.sourceFile.Replace(info.Extension, ".bgms");
                }

                int totalScanCount = reader.getTotalScanCount();
                updateProgress(info.Name, 0, totalScanCount);

                bool allFine = true;

                BGSRawAPI.BGSRawFileWriter writer = new BGSRawAPI.BGSRawFileWriter(reader.getVendor().ToString(), reader.getInstrumentModel(), reader.getInstrumentSerialNumber(), 
                    reader.getAcquisitionDate(), BGSMSEnums.MSMethodType.UNKNOWN, reader.getOriginalFileName(), this.destinationFile, false, true);

                try {
                    int scanIndex = 0;
                    foreach (BGSRawAPI.ScanEvent scan in reader) {
                        writer.AddScan(scan);
                        ++scanIndex;

                        this.updateProgress(info.Name, scanIndex, totalScanCount);
                    }
                } catch (Exception ex) {
                    Console.WriteLine("Error: " + ex.Message);
                    allFine = false;
                }
                writer.Finalize();
                reader.close();
                string status = (allFine) ? " [DONE]" : " [FAILED]";

                Console.WriteLine(consoleClean + info.Name + status);
                if (allFine) {
                    return this.destinationFile;
                }
            }

            return null;
        }

        private int progressCount = 0;
        private void updateProgress(string fileName, int scanIndex, int totalScanCount) {
            if (totalScanCount > 0) {
                int updateStep = Math.Max(1, totalScanCount / 212);
                if (scanIndex % updateStep == 0) {
                    double progress = Math.Round(((double)scanIndex / (double)totalScanCount) * 100.0, 1);
                    Console.Write(consoleClean + "Processing: " + fileName + " [" + progress +"%]");
                }
            } else {
                if (scanIndex % 100 == 0) {
                    Console.Write(consoleClean + "Processing: " + fileName + " [" + progressIndicators[progressCount % progressIndicators.Length]+"]");
                    ++this.progressCount;
                }
            }
        }
    }
}
