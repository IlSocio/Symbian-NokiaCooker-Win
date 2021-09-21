using System;
using System.Collections.Generic;
using System.Text;
using FuzzyByte.Utils;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Firmware
{
    public class TSectionArea
    {
        public List<TSectionInfo> sectionsList = new List<TSectionInfo>();

        public TSectionArea(TBlock sectionBlock)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(sectionBlock.content));
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                TSectionInfo sec = new TSectionInfo();
                sec.Read(br);
                sectionsList.Add(sec);
//                if (sec.startAddress != 0xFFFFFFFF)
//                    Debug.WriteLine(sec);
            }
        }

        public UInt32 ExtendRofs1()
        {
            List<TSectionInfo> sections = GetRofsSections();
            if (sections.Count < 2)
                throw new FwException("It is not possible to extend ROFS1 size for this fw! Please contact m.bellino@symbian-toys.com");
            TSectionInfo rofs1 = sections[0];
            TSectionInfo rofs2 = sections[1];
            if (rofs1 == null || rofs2 == null)
                throw new FwException("It is not possible to extend ROFS1 size for this fw! Please contact m.bellino@symbian-toys.com");
            UInt64 newLen = rofs2.startAddress - rofs1.startAddress;
            UInt64 oldLen = rofs1.length;
            if (rofs1.length == newLen)
                return 0;
            if (newLen < rofs1.length)
                throw new FwException("The ROFS1 Extended size is < than the current size! Please contact m.bellino@symbian-toys.com");
            rofs1.length = rofs2.startAddress - rofs1.startAddress;
            return (UInt32)(newLen - oldLen);
        }

        public TSectionInfo GetUserSection()
        {
            foreach (TSectionInfo info in sectionsList)
            {
                string s = BytesUtils.ToString(info.descr).ToUpper();
                if (s.StartsWith("SOS") && s.Contains("USER"))
                    return info;
            }
            return null;
        }

        public List<TSectionInfo> GetRofsSections()
        {
            List<TSectionInfo> res = new List<TSectionInfo>();
            foreach (TSectionInfo info in sectionsList)
            {
                Debug.WriteLine(info);
                string s = BytesUtils.ToString(info.descr).ToUpper();
                if (s.StartsWith("SOS") && s.Contains("ROF"))
                    res.Add(info);
            }
            return res;
        }

        public void WriteToSectionAreaBlock(TBlock sectionBlock)
        {
            BinaryWriter bw = new BinaryWriter(new MemoryStream(sectionBlock.content));
            foreach (TSectionInfo sect in sectionsList)
            {
                sect.Write(bw);
            }
        }

        public override string ToString()
        {
            string s = "SectionArea:" + Environment.NewLine;
            foreach (TSectionInfo sect in sectionsList)
            {
                s = sect.ToString() + Environment.NewLine;
            }
            return s;
        }
    }


    public class TSectionInfo
    {
        public UInt32 startAddress;
        public UInt32 length;
        public byte[] unkn = null;
        public byte[] descr = null;

        public UInt32 EndAddress
        {
            get
            {
                return startAddress + length;
            }
        }

        public string Descr
        {
            get
            {
                string s = BytesUtils.ToString(descr).ToUpper();
                return s;
            }
        }

        public bool GetProtValue(out byte value)
        {
            value = 0;
            int protPos = Descr.IndexOf("ROF") - 1;
            if (protPos < 0)
                return false;
            value = descr[protPos];
            return true;
        }

        public bool SetProtValue(byte value)
        {
            int protPos = Descr.IndexOf("ROF") - 1;
            if (protPos < 0)
                return false;
            descr[protPos] = value;
            return true;
        }

        public void Read(BinaryReader br)
        {
            startAddress = br.ReadUInt32();
            length = br.ReadUInt32();
            unkn = br.ReadBytes(12);
            descr = br.ReadBytes(12);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(startAddress);
            bw.Write(length);
            bw.Write(unkn);
            bw.Write(descr);
        }

        public override string ToString()
        {
            string s = Descr + "\t";
            s += BytesUtils.ToHex(startAddress) + " - ";
            s += BytesUtils.ToHex(EndAddress);
            s += "  " + BytesUtils.ToHex(unkn);
            return s;
        }
    }

}
