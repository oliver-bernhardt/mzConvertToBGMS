using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGSRawAPI;
using BGSMSEnums;

namespace MZConvertToBGMS.MSScanReader {
    public abstract class AScanReader : IEnumerable<ScanEvent> {

        public static AScanReader getScanReader(string file) {
            if (file.ToLower().EndsWith(".mzxml")) {
                return new MzXmlScanReader(file);
            }
            return null;
        }

        public abstract IEnumerator<ScanEvent> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return (System.Collections.IEnumerator)GetEnumerator();
        }

        public abstract string getOriginalFileName();
        public abstract string getInstrumentModel();
        public abstract string getInstrumentSerialNumber();
        public abstract DateTime getAcquisitionDate();
        public abstract VendorType getVendor();
        public abstract void close();
        public abstract int getTotalScanCount();
    }
}
