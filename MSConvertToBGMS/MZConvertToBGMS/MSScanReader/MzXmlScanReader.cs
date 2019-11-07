using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGSMSEnums;
using BGSRawAPI;
using System.Xml;
using System.Net;

namespace MZConvertToBGMS.MSScanReader {
    public class MzXmlScanReader : AScanReader {
        private static readonly string SCAN_NODE_NAME = "scan";
        private static readonly string PEAKS_NODE_NAME = "peaks";
        private static readonly string PRECURSOR_NODE_NAME = "precursorMz";
        private static readonly string VENDOR_NODE_NAME = "msManufacturer";
        private static readonly string MSMODEL_NODE_NAME = "msModel";
        private static readonly string MASS_ANALYZER_NODE_NAME = "msMassAnalyzer";
        private static readonly string PARENT_FILE_NODE_NAME = "parentFile";
        private static readonly string MSRUN_NODE_NAME = "msRun";

        private static readonly string ISCENTROID_ATTRIBUTE_NAME = "centroided";
        private static readonly string MSLEVEL_ATTRIBUTE_NAME = "msLevel";
        private static readonly string RT_ATTRIBUTE_NAME = "retentionTime";
        private static readonly string WINDOW_WIDTH_ATTRIBUTE_NAME = "windowWideness";
        private static readonly string COMPRESSION_TYPE_ATTRIBUTE_NAME = "compressionType";
        private static readonly string PRECISSION_TYPE_ATTRIBUTE_NAME = "precision";
        private static readonly string BYTE_ORDER_TYPE_ATTRIBUTE_NAME = "byteOrder";
        private static readonly string SCAN_COUNT_ATTRIBUTE_NAME = "scanCount";

        private XmlReader reader;

        private VendorType vendor = VendorType.Unknown;
        private string msModel = "Unknown";
        private string msSerialNumber = "Unknown";
        private DateTime acquisitionDate = DateTime.Now;
        private string parentFile = "Unknown";
        private int totalScanCount = -1;

        public MzXmlScanReader(string mzXmlFile) {
            System.IO.Stream fStream = new System.IO.FileStream(mzXmlFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
            XmlReaderSettings settings = new XmlReaderSettings();
            this.reader = XmlReader.Create(fStream, settings);
            this.readHeader();
        }

        public override void close() {
            this.reader.Close();
            this.reader = null;
        }

        private void readHeader() {
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element) {
                    if (reader.Name.Equals(VENDOR_NODE_NAME, StringComparison.OrdinalIgnoreCase)) {
                        this.vendor = parseVendor(reader.GetAttribute("value"));
                    } else if (reader.Name.Equals(MSMODEL_NODE_NAME, StringComparison.OrdinalIgnoreCase)) {
                        this.msModel = reader.GetAttribute("value");
                    } else if (reader.Name.Equals(PARENT_FILE_NODE_NAME, StringComparison.OrdinalIgnoreCase)) {
                        this.parentFile = reader.GetAttribute("fileName");
                    } else if (reader.Name.Equals(MSRUN_NODE_NAME, StringComparison.OrdinalIgnoreCase)) {
                        string scanCountValue = reader.GetAttribute(SCAN_COUNT_ATTRIBUTE_NAME);
                        int count;
                        if (!string.IsNullOrEmpty(scanCountValue) && int.TryParse(scanCountValue, out count)) {
                            this.totalScanCount = count;
                        }
                    }
                } else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("msInstrument", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }
            }
        }

        private static VendorType parseVendor(string vendorString) {
            string vsLower = vendorString.ToLower();
            foreach (VendorType vendor in Enum.GetValues(typeof(VendorType))) {
                string typeString = vendor.ToString().ToLower();
                if (vsLower.Contains(typeString)) {
                    return vendor;
                }
                if (vendor == VendorType.Absciex && vsLower.Contains("sciex")) {
                    return VendorType.Absciex;
                }
            }
            return VendorType.Unknown;
        }

        private ScanEvent readNextScan() {
            ScanMode scanMode = ScanMode.Unknown;
            MSLevel msLevel = MSLevel.UNKNOWN;
            double rt = double.NaN;
            double mzWindowStart = 0.0;
            double mzWindowEnd = 5000;
            float[,] scan = null;

            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element) {
                    if (reader.Name.Equals(SCAN_NODE_NAME, StringComparison.OrdinalIgnoreCase)) {//Read <scan> Node
                        scanMode = readScanMode(this.reader);
                        msLevel = readMSLevel(this.reader);
                        rt = readRTInMinutes(this.reader);

                    } else if (reader.Name.Equals(PEAKS_NODE_NAME, StringComparison.OrdinalIgnoreCase)) { //Read <peaks> Node
                        string compressionType = reader.GetAttribute(COMPRESSION_TYPE_ATTRIBUTE_NAME);
                        string precissionType = reader.GetAttribute(PRECISSION_TYPE_ATTRIBUTE_NAME);
                        string byteOrderType = reader.GetAttribute(BYTE_ORDER_TYPE_ATTRIBUTE_NAME);
                        reader.Read();
                        byte[] scanData = System.Convert.FromBase64String(reader.Value);
                        bool reversedOrder = byteOrderType.Equals("network", StringComparison.OrdinalIgnoreCase);



                        if (compressionType.Equals("zlib", StringComparison.OrdinalIgnoreCase)) {
                            scanData = Util.DecompressZLib(scanData);
                        } else if (compressionType.Equals("none", StringComparison.OrdinalIgnoreCase)) {
                            //Nothing todo in this case
                        } else {
                            throw new FormatException("Not supported compression type: " + compressionType);
                        }

                        if (reversedOrder) {
                            Array.Reverse(scanData);
                        }


                        if (precissionType.Equals("64")) {
                            double[] dScan = Util.readDoubleArray(scanData);
                            if (reversedOrder) { Array.Reverse(dScan); }
                            scan = Util.mzPairsToScan(dScan);
                        } else {
                            float[] fScan = Util.readFloatArray(scanData);
                            if (reversedOrder) { Array.Reverse(fScan); }
                            scan = Util.mzPairsToScan(fScan);
                        }


                    } else if (reader.Name.Equals(PRECURSOR_NODE_NAME, StringComparison.OrdinalIgnoreCase)) { //Read <precursorMz> Node
                        string widthString = reader.GetAttribute(WINDOW_WIDTH_ATTRIBUTE_NAME);
                        reader.Read();
                        string windowCenterString = reader.Value;
                        double center; double width;
                        if (double.TryParse(widthString, out width) && double.TryParse(windowCenterString, out center)) {
                            mzWindowStart = center - (width / 2.0);
                            mzWindowEnd = center + (width / 2.0);
                        }
                    }
                } else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(SCAN_NODE_NAME)) {
                    bool isCentroid = scanMode != ScanMode.Profile;
                    PrecursorSelection[] window = new PrecursorSelection[] { new PrecursorSelection(mzWindowStart, mzWindowEnd) };
                    return new ScanEvent(scan, rt, msLevel, isCentroid, window);
                }
            }

            return null;
        }


        /// <summary>
        /// Extracts the attribute containing the flag for "IsCentroid" and returns the corresponding ScanMode enum
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ScanMode readScanMode(XmlReader reader) {
            ScanMode mode = ScanMode.Unknown;
            string value = reader.GetAttribute(ISCENTROID_ATTRIBUTE_NAME);
            if (!string.IsNullOrEmpty(value)) {
                bool isCentroid;
                if (!bool.TryParse(value, out isCentroid)) {
                    isCentroid = !value.Equals("0"); //If it is not explicitly marked as PROFILE, we will assume centroid mode (its the safer option in Spectronaut if unknown)
                }

                return (isCentroid) ? ScanMode.Centroid : ScanMode.Profile;
            }
            return mode;
        }

        /// <summary>
        /// Extracts the attribute containing the flag for "MSLevel" (MS1 or MS2) and returns the corresponding MSLevel enum
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private MSLevel readMSLevel(XmlReader reader) {
            MSLevel mode = MSLevel.UNKNOWN;
            string value = reader.GetAttribute(MSLEVEL_ATTRIBUTE_NAME);
            int msOrder;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out msOrder)) {
                mode = (MSLevel)(msOrder - 1);
            }
            return mode;
        }

        /// <summary>
        /// Extracts the attribute containing the RT annotation for a scan and returns the retention time in minutes
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private double readRTInMinutes(XmlReader reader) {
            string value = reader.GetAttribute(RT_ATTRIBUTE_NAME).ToLower();
            char unit = value[value.Length - 1];
            value = removeAllNonDigitChars(value);

            double rt;
            if (double.TryParse(value, out rt)) {
                switch (unit) {
                    case 's':
                        return rt / 60.0;
                    case 'm':
                        return rt;
                    case 'h':
                        return rt * 60.0;
                    default:
                        return rt;
                }
            } else {
                throw new FormatException("Failed to parse RT " + value.ToString() + "as numeric value");
            }
        }

        /// <summary>
        /// RT annotation in mzXML files is started with "PT" and ends with "S" (which I assume stands for the unit in seconds).
        /// Both of them need to be removed in order to parse the string into a double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string removeAllNonDigitChars(string value) {
            char decimalSign = Convert.ToChar(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; ++i) {
                if ((value[i] >= (int)'0' && value[i] <= (int)'9') || value[i].Equals(decimalSign)) {
                    builder.Append(value[i]);
                }
            }
            return builder.ToString();
        }


        public override DateTime getAcquisitionDate() {
            return this.acquisitionDate;
        }

        public override IEnumerator<ScanEvent> GetEnumerator() {
            ScanEvent scan = null;
            while ((scan = this.readNextScan()) != null) {
                yield return scan;
            }
            yield break;
        }

        public override string getInstrumentModel() {
            return this.msModel;
        }

        public override string getInstrumentSerialNumber() {
            return this.msSerialNumber;
        }

        public override string getOriginalFileName() {
            return this.parentFile;
        }

        public override VendorType getVendor() {
            return this.vendor;
        }

        public override int getTotalScanCount() {
            return this.totalScanCount;
        }

        public override MassAnalyzerType getMassAnalyzerType(MSLevel level) {
            return MassAnalyzerType.Unknown;
        }
    }
}

