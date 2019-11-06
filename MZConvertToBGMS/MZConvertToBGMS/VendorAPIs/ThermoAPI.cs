using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

namespace MZConvertToBGMS.VendorAPIs {
    public class ThermoAPI {
        private string file;
        internal IRawDataPlus rawFileReader = null;

        public ThermoAPI(string file) {
            this.file = file;
            this.open(file);
        }
        private void open(string filename) {
            if (this.rawFileReader == null) {
                IRawDataPlus rawFile = RawFileReaderAdapter.FileFactory(filename);
                rawFile.SelectInstrument(Device.MS, 1);
                this.rawFileReader = rawFile;
            }
        }

        public void close() {
            if (this.rawFileReader != null) {
                this.rawFileReader.Dispose();
                this.rawFileReader = null;
            }
        }

        public int getNumberOfScans() {
            return this.rawFileReader.RunHeaderEx.SpectraCount;
        }


        public double[][] getScan2(int scan) {
            double[][] raw = null;
            try {
                ISimpleScanAccess scanObject = this.rawFileReader.GetSimplifiedScan(scan);

                raw = new double[2][];
                raw[0] = scanObject.Masses;
                raw[1] = scanObject.Intensities;
            } catch (Exception ex) {
                raw = new double[2][];
                raw[0] = new double[0];
                raw[1] = new double[0];
                Console.WriteLine("Error while reading Thermo Scan: "+ex.Message);
            } 
            return raw;
        }

        public double[,] getScan(int scan) {
            double[][] raw2 = getScan2(scan);
            double[,] raw = new double[2, raw2[0].Length];

            System.Buffer.BlockCopy(raw2[0], 0, raw, 0, sizeof(double) * raw2[0].Length);
            System.Buffer.BlockCopy(raw2[1], 0, raw, sizeof(double) * raw2[0].Length, sizeof(double) * raw2[0].Length);

            return raw;
        }


        public bool IsCentroidScan(int scan) {
            try {
                return this.rawFileReader.IsCentroidScanFromScanNumber(scan);
            } catch { }
            return false;
        }

        public BGSMSEnums.MassAnalyzerType getMassAnalyzerType(int scannr) {
            IScanEvent scan = rawFileReader.GetScanEventForScanNumber(scannr);
            return (BGSMSEnums.MassAnalyzerType)scan.MassAnalyzer;
        }

        public double[,] getWindowDefinitionsForScanNumber(int scan) {
            IScanEvent scanEvent = this.rawFileReader.GetScanEventForScanNumber(scan);
            List<Tuple<double, double>> massRanges = new List<Tuple<double, double>>();
            int msOrder = this.getMSOrderForScanNumber(scan);

            if (msOrder == 1 && scanEvent.MassCount == 0) {
                for (int i = 0; i < scanEvent.MassRangeCount; ++i) {
                    Range r = scanEvent.GetMassRange(i);
                    massRanges.Add(new Tuple<double, double>(r.Low, r.High));
                }
            } else {
                for (int i = 0; i < scanEvent.MassCount; ++i) {
                    try {
                        IReaction reaction = scanEvent.GetReaction(i);
                        double center = reaction.PrecursorMass;
                        double width = reaction.IsolationWidth;
                        double start = reaction.FirstPrecursorMass;
                        double end = reaction.LastPrecursorMass;

                        double start2 = center - width / 2.0;
                        double end2 = start2 + width;
                        if (Math.Abs(start2 - end2) > 0) {
                            massRanges.Add(new Tuple<double, double>(start2, end2));
                        } else {
                            massRanges.Add(new Tuple<double, double>(start, end));
                        }

                    } catch { }
                }
            }
            double[,] masses = new double[2, massRanges.Count];
            for (int i = 0; i < massRanges.Count; ++i) {
                masses[0, i] = massRanges[i].Item1;
                masses[1, i] = massRanges[i].Item2;
            }
            return masses;
        }



        public DateTime getAcquisitionDate() {
            return this.rawFileReader.CreationDate;
        }

        public String getInstName() {
            return this.rawFileReader.GetInstrumentData().Name;
        }

        public double getRTFromScanNumber(int scan) {
            return this.rawFileReader.RetentionTimeFromScanNumber(scan);
        }

        public int getMSOrderForScanNumber(int scan) {
            IScanEvent scanEvent = this.rawFileReader.GetScanEventForScanNumber(scan);
            return (int)scanEvent.MSOrder;
        }

        public string getInstModel() {
            return this.rawFileReader.GetInstrumentData().Model;
        }

        public string getSoftwareVersion() {
            return this.rawFileReader.GetInstrumentData().SoftwareVersion;
        }

        public string getHardwareVersion() {
            return this.rawFileReader.GetInstrumentData().HardwareVersion;
        }

        public string getInstSerialNumber() {
            return this.rawFileReader.GetInstrumentData().SerialNumber;
        }

    }
}
