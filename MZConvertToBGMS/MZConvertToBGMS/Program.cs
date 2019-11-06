using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZConvertToBGMS {
    public class Program {
        public static void Main(string[] args) {
            string file = @"F:\Data\HRM_GS\HRMv5\raw\B_D140314_SGSDSsample1_R01_MHRM_T0.mzXML";
            MSScanReader.MzXmlScanReader reader = new MSScanReader.MzXmlScanReader(file);

            BGSRawAPI.BGSRawFileWriter writer = new BGSRawAPI.BGSRawFileWriter(reader.getVendor().ToString(), reader.getInstrumentModel(), reader.getInstrumentSerialNumber(), reader.getAcquisitionDate(), BGSMSEnums.MSMethodType.HRM,  reader.getOriginalFileName(), file + ".bgms", false, true );
            foreach (BGSRawAPI.ScanEvent scan in reader) {
                writer.AddScan(scan);
            }
            writer.Finalize();
        }
    }
}
