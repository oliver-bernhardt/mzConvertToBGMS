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

        /// <summary>
        /// tries to convert the currently selected file if it is of one of the supported input types.
        /// returns the converted files location on success or null if it failed to convert the file.
        /// </summary>
        /// <returns></returns>
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

                //Currently, I keep the method type as UNKOWN. that works fine for Spectronaut but not necessarily for SpectroMine or SpectroDive
                BGSRawAPI.BGSRawFileWriter writer = new BGSRawAPI.BGSRawFileWriter(reader.getVendor().ToString(), reader.getInstrumentModel(), reader.getInstrumentSerialNumber(), 
                    reader.getAcquisitionDate(), BGSMSEnums.MSMethodType.UNKNOWN, reader.getOriginalFileName(), this.destinationFile, false, true);

                writer.MetaData[BGSRawAPI.BGSRawFileWriter.MS1_MASS_ANALYZER] = reader.getMassAnalyzerType(BGSMSEnums.MSLevel.MS1).ToString();
                writer.MetaData[BGSRawAPI.BGSRawFileWriter.MS2_MASS_ANALYZER] = reader.getMassAnalyzerType(BGSMSEnums.MSLevel.MS2).ToString();

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
        /// <summary>
        /// updates the progress in the console.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="scanIndex"></param>
        /// <param name="totalScanCount"></param>
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
