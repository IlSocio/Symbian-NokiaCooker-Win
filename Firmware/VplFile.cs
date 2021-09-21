using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Firmware
{
    public class VplFile
    {
        public static List<VplEntry> GetAllEntries(string vplFile)
        {
            List<VplEntry> res = new List<VplEntry>();
            using (XmlTextReader reader = new XmlTextReader(vplFile))
            {
                while (reader.Read())
                {
                    reader.ReadToFollowing("File");
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;
                    XmlReader rdr = reader.ReadSubtree();
                    VplEntry entry = new VplEntry();
                    while (rdr.Read())
                    {
                        if (rdr.NodeType != XmlNodeType.Element)
                            continue;
                        string val = rdr.Name.ToLower();
                        if (val == "filesubtype")
                        {
                            rdr.Read();
                            switch (rdr.Value.ToLower().Trim())
                            {
                                case "ppm":
                                    entry.fileType = TFileType.PPM;
                                    break;
                                case "content":
                                    entry.fileType = TFileType.CONTENT;
                                    break;
                                case "mcu":
                                    entry.fileType = TFileType.MCU;
                                    break;
                            }                            
                            continue;
                        }
                        if (val == "crc")
                        {
                            rdr.Read();
                            entry.crc = rdr.Value.ToLower().Trim().Replace("0x", "");
                            continue;
                        }
                        if (val == "optional")
                        {
                            rdr.Read();
                            entry.optional = (rdr.Value.ToLower().Trim() == "true");
                            continue;
                        }
                        if (val == "name")
                        {
                            rdr.Read();
                            entry.filename = rdr.Value.Trim();
                            res.Add(entry);
                            continue;
                        }
                    }
                }
            }
            return res;
        }

        public static bool GetCrcForFile(string vplFile, string filename, out UInt32 crc)
        {
            crc = 0;
            string myFile = Path.GetFileName(filename.ToLower().Trim());

            List<VplEntry> entries = GetAllEntries(vplFile);
            foreach (VplEntry entry in entries)
            {
                if ((entry.filename.ToLower().Trim() == myFile) && (entry.crc != ""))
                {
                    bool ok = UInt32.TryParse(entry.crc, System.Globalization.NumberStyles.AllowHexSpecifier, null, out crc);
                    return ok;
                }
            }
            return false;
        }
    }

    public enum TFileType
    {
        MCU = 0,
        PPM,
        CONTENT,
        OTHER
    }

    public class VplEntry
    {
        public string filename = "";
        public bool optional = true;
        public TFileType fileType = TFileType.OTHER;
        public string crc = "";
    }
}
