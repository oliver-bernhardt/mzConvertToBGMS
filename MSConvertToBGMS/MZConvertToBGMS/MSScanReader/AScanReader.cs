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
            } else if (file.ToLower().EndsWith(".raw")) {
                //return new ThermoScanReader(file);
                //Support for thermo is currently disabled because I don't want to distribute the RawFileReader dlls
                //To enable thermo support you just need to include the classes "ThermoScanReader.cs" and "ThermoAPI.cs"
                //and include the RawFileReader dlls into the list of references.
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

        //Returns the mass analyzer (Orbitrap, TOF, etc.) that was used per MSLevel (MS1 or MS2)
        //If you don't know the mass analyzer that was used, simply return MassAnalyzerType.UNKNOWN
        public abstract MassAnalyzerType getMassAnalyzerType(MSLevel level);

        /// <summary>
        /// implement if available to get the progress in %
        /// </summary>
        /// <returns></returns>
        public virtual int getTotalScanCount() {
            return -1;
        }
    }
}
