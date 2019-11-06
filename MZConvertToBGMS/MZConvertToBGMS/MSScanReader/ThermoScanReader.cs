using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGSMSEnums;
using BGSRawAPI;
using MZConvertToBGMS.VendorAPIs;

namespace MZConvertToBGMS.MSScanReader {
    public class ThermoScanReader : AScanReader {
        private ThermoAPI api;
        private string file;
        public ThermoScanReader(string file) {
            this.file = file;
            this.api = new ThermoAPI(file);
        }

        public override void close() {
            this.api.close();
            this.api = null;
        }

        public override DateTime getAcquisitionDate() {
            return this.api.getAcquisitionDate();
        }

        public override IEnumerator<ScanEvent> GetEnumerator() {
            for (int i = 1; i <= this.api.getNumberOfScans(); ++i) {
                double[,] msxWindows = this.api.getWindowDefinitionsForScanNumber(i);
                double rt = this.api.getRTFromScanNumber(i);
                MSLevel level = (MSLevel)this.api.getMSOrderForScanNumber(i);
                bool isCentroid = this.api.IsCentroidScan(i);

                double[,] scan = this.api.getScan(i);

                PrecursorSelection[] windows = new PrecursorSelection[msxWindows.GetLength(1)];
                for (int j = 0; j < windows.Length; ++j) {
                    windows[j] = new PrecursorSelection(msxWindows[0, j], msxWindows[1, j]);
                }

                yield return new ScanEvent(scan, rt, level, isCentroid, windows);
            }

            yield break;
        }

        public override string getInstrumentModel() {
            return this.api.getInstModel();
        }

        public override string getInstrumentSerialNumber() {
            return this.api.getInstSerialNumber();
        }

        public override string getOriginalFileName() {
            return this.file;
        }

        public override VendorType getVendor() {
            return VendorType.Thermo;
        }

        public override int getTotalScanCount() {
            return this.api.getNumberOfScans();
        }
    }
}
