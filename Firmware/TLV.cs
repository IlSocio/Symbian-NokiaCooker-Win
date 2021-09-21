using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;


namespace Firmware
{

    public class TLV
    {
        public UInt32 _offset = 0;
        public TTLVType type;
        public int length;
        public byte[] value = new byte[0];

        protected TLV(TTLVType aType)
        {
            type = aType;
        }

        protected TLV(TTLVType aType, BinaryReader br)
        {
            _offset = (UInt32)br.BaseStream.Position;
            type = aType;
            length = br.ReadByte();
            value = br.ReadBytes(length);
        }

        public static TLV Factory(BinaryReader br, bool supportPartition)
        {
            TTLVType type = (TTLVType)br.ReadByte();
            switch (type)
            {
                case TTLVType.AlgoSendingSpeed:
                    break;
                case TTLVType.DateTime:
                    return new TLV_DateTime(br);
                case TTLVType.CMT_Algo:
                    return new TLV_String(type, br);
                case TTLVType.CMT_Type:
                    return new TLV_String(type, br);
                case TTLVType.Descr:
                    return new TLV_String(type, br);
                case TTLVType.Array:
                    return new TLV_EE_Array(br, supportPartition);
                case TTLVType.UnknE5:
                    return new TLV_E5(br);
                case TTLVType.UnknEA:
                    return new TLV_EA(br);
                case TTLVType.UnknF3_Imp:
                    return new TLV_F3(br);/**/
                case TTLVType.UnknE4_Imp:
                    return new TLV_E4(br);
                case TTLVType.UnknFA_Imp:
                    return new TLV_FA(br);
                case TTLVType.ERASE_DCT5:
                    return new TLV_Erase_DCT5(br, supportPartition);
                case TTLVType.ERASE_AREA_BB5:
                    return new TLV_Erase_Area_BB5(br);
#if DEBUG
                case TTLVType.FORMAT_PARTITION_BB5:                 // Formattazione PPM
                    return new deb_TLV_Format_Partition_BB5(br);        // Ancora da gestire
                case TTLVType.PARTITION_INFO_BB5:
                    return new TLV_Partition_Info_BB5(br);          // Per sicurezza disattiva il parsing perche' il tipo MMC non e' gestito
#else
                case TTLVType.PARTITION_INFO_BB5:
                    if (supportPartition)
                        return new TLV_Partition_Info_BB5(br);          // Per sicurezza disattiva il parsing perche' il tipo MMC non e' gestito
                    else
                        return new TLV(type, br);
#endif
                default:
                    break;
            }
            return new TLV(type, br);
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write((byte)type);
            bw.Write((byte)length);
            bw.Write(value);
        }

#if DEBUG  
        public override string ToString()
        {
            return type.ToString() + ": " + BytesUtils.ToHex(value);
        }
#endif
    }

    public class TLV_FA : TLV
    {
        public TLV_FA(BinaryReader br)
            : base(TTLVType.UnknFA_Imp)
        {
            _offset = (UInt32)br.BaseStream.Position;
            byte trash = br.ReadByte();
            if (trash != 0)
                throw new FwException("FA Len not zero");
            length = (int)BytesUtils.SwapBytes(br.ReadUInt32());
            value = br.ReadBytes(length);
        }

#if DEBUG
        public override string ToString()
        {
            return type.ToString() + ": Too much long";
        }
#endif

        public override void Write(BinaryWriter bw)
        {
            UInt32 newLen = BytesUtils.SwapBytes((UInt32)length);
            bw.Write((byte)type);
            bw.Write((byte)0);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write(value);
        }
    }

    public class TLV_EA : TLV
    {
        public TLV_EA(BinaryReader br)
            : base(TTLVType.UnknEA)
        {
            _offset = (UInt32)br.BaseStream.Position;
            // Si, sembra corretto perche' RM-504_32.0.007_prd.core.c00 riporta una length congruente           
            length = BytesUtils.SwapBytes(br.ReadUInt16());
            value = br.ReadBytes(length);
        }
        public override void Write(BinaryWriter bw)
        {
            UInt16 newLen = BytesUtils.SwapBytes((UInt16)length);
            bw.Write((byte)type);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write(value);
        }
    }

    public class TLV_E5 : TLV
    {
        public TLV_E5(BinaryReader br)
            : base(TTLVType.UnknE5)
        {
            _offset = (UInt32)br.BaseStream.Position;
            // Si, sembra corretto perche' RM-504_32.0.007_prd.core.c00 riporta una length congruente           
            length = BytesUtils.SwapBytes(br.ReadUInt16());
            value = br.ReadBytes(length);
        }
        public override void Write(BinaryWriter bw)
        {
            UInt16 newLen = BytesUtils.SwapBytes((UInt16)length);
            bw.Write((byte)type);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write(value);
        }
    }

    public class TLV_F3 : TLV
    {
        public TLV_F3(BinaryReader br)
            : base(TTLVType.UnknF3_Imp)
        {
            _offset = (UInt32)br.BaseStream.Position;
            // Si, sembra corretto perche' RM-504_32.0.007_prd.core.c00 riporta una length congruente           
            length = BytesUtils.SwapBytes(br.ReadUInt16());  
            value = br.ReadBytes(length);
        }
        public override void Write(BinaryWriter bw)
        {
            UInt16 newLen = BytesUtils.SwapBytes((UInt16)length);
            bw.Write((byte)type);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write(value);
        }
    }

    public class TLV_E4 : TLV
    {
        // (A quanto pare e' contenuto dentro F3)
        public TLV_E4(BinaryReader br)
            : base(TTLVType.UnknE4_Imp)
        {
            _offset = (UInt32)br.BaseStream.Position;
            length = BytesUtils.SwapBytes(br.ReadUInt16());
            value = br.ReadBytes(length);
        }
        public override void Write(BinaryWriter bw)
        {
            UInt16 newLen = BytesUtils.SwapBytes((UInt16)length);
            bw.Write((byte)type);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write(value);
        }
    }


    public class TLV_EE_Array : TLV
    {
        public List<TLV> array = new List<TLV>();

        public TLV_EE_Array(BinaryReader br, bool supportPartitions)
            : base(TTLVType.Array)
        {
            _offset = (UInt32)br.BaseStream.Position;
            // Un esempio completo e' (N96) RM-247_30.033_prd.core
            // c'e' una length > 256 e ci sono 2 tabelle delle partizioni
            length = BytesUtils.SwapBytes(br.ReadUInt16());
            value = br.ReadBytes(length);

            BinaryReader binRead = new BinaryReader(new MemoryStream(value));
            byte count = binRead.ReadByte();
            for (int i = 0; i < count; i++)
            {
                TLV newTlv = TLV.Factory(binRead, supportPartitions);
                array.Add(newTlv);
            }
            binRead.Close();
        }

        public override void Write(BinaryWriter bw)
        {
            UInt16 newLen = BytesUtils.SwapBytes((UInt16)length);
            bw.Write((byte)type);
            bw.Write(newLen);   // TODO: Verifica se l'ordine dei byte e' corretto.
            bw.Write((byte)array.Count);
            foreach (TLV tlv in array)
            {
                tlv.Write(bw);
            }
        }

        public override string ToString()
        {
            string s = base.ToString();
            foreach (TLV aElem in array)
            {
                s += Environment.NewLine + "\t";
                s += aElem.ToString();
            }
            return s;
        }
    }


    public class TLV_Erase_DCT5 : TLV
    {
        // N95 ha piu' di una Erase Area BB5..
        public List<TLV> operations = new List<TLV>();

        public TLV_Erase_DCT5(BinaryReader br, bool supportPartition)
            : base(TTLVType.ERASE_DCT5, br)
        {
            BinaryReader binRead = new BinaryReader(new MemoryStream(value));
            int qtyBlocks = binRead.ReadByte();
            for (int j = 0; j < qtyBlocks; j++)
            {
                TLV tlv = TLV.Factory(binRead, supportPartition);
                operations.Add(tlv);
            }
            binRead.Close();
        }

        public override void Write(BinaryWriter bw)
        {
            BinaryWriter binWrite = new BinaryWriter(new MemoryStream(value));
            binWrite.Write((byte)operations.Count);
            foreach (TLV oper in operations)
            {
                oper.Write(binWrite);
            }
            binWrite.Close();
            base.Write(bw);
        }

        public override string ToString()
        {
            string s = base.ToString();
            foreach (TLV aOper in operations)
            {
                s += Environment.NewLine + "\t";
                s += aOper.ToString();
                TLV_Erase_Area_BB5 erArea = aOper as TLV_Erase_Area_BB5;
                if (erArea != null)
                   s += Environment.NewLine + "\tTot Erase Area: " + erArea.TotAreaErase();
            }
            return s;
        }
    }
#if DEBUG
#endif


    public class TLV_DateTime : TLV
    {
        public DateTime datetime;

        public TLV_DateTime(BinaryReader br)
            : base(TTLVType.DateTime, br)
        {
            // Convert Unix TimeStamp to .NET
            UInt32 intVal = BytesUtils.BuildInt(value[0], value[1], value[2], value[3]);
            datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            datetime = datetime.AddSeconds(intVal);
        }

        public override string ToString()
        {
            string descr = datetime.ToShortDateString() + "  " + datetime.ToShortTimeString();
            return base.ToString() + Environment.NewLine + "\t" + descr;
        }
    }


    public class TLV_String : TLV
    {
        public string descr;

        public TLV_String(TTLVType aType, BinaryReader br)
            : base(aType, br)
        {
            descr = ASCIIEncoding.ASCII.GetString(value);
            descr = descr.Replace("\0", "\n");
            if (descr.Length > 0)
                descr = descr.Remove(descr.Length - 1, 1);
        }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + "\t" + descr;
        }
    }


    public class TEraseItem
    { 
        public UInt32 start;
        public UInt32 end;

        public TEraseItem(BinaryReader br)
        {
            start = BytesUtils.SwapBytes(br.ReadUInt32());
            end = BytesUtils.SwapBytes(br.ReadUInt32());
        }

        public bool ContainAddr(UInt32 addr)
        {
            return (addr >= start && addr <= end);
        }

        public UInt32 Size
        {
            get
            {
                return end - start;
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(BytesUtils.SwapBytes(start));
            bw.Write(BytesUtils.SwapBytes(end));
        }

        public override string ToString()
        {
            return BytesUtils.ToHex(start) + " - " + BytesUtils.ToHex(end) + "  Size: " + BytesUtils.GetSizeDescr(Size);
        }
    }


    public class TLV_Erase_Area_BB5 : TLV
    {
        public TASICType asicType;           // 0x00 CMT
        public TDeviceType deviceType;     // MuxOneNAND
        public byte deviceIndex;
        public List<TEraseItem> eraseList = new List<TEraseItem>();

        public TLV_Erase_Area_BB5(BinaryReader br)
            : base(TTLVType.ERASE_AREA_BB5, br)
        {
            BinaryReader binRead = new BinaryReader(new MemoryStream(value));

            asicType = (TASICType)binRead.ReadByte();
            deviceType = (TDeviceType)binRead.ReadByte();
            deviceIndex = binRead.ReadByte();
            int qtaAddresses = (value.Length - 3) / 8;
            for (int i = 0; i < qtaAddresses; i++)
            {
                TEraseItem erItem = new TEraseItem(binRead);
                eraseList.Add(erItem);
            }
        }

        public override void Write(BinaryWriter bw)
        {
            BinaryWriter binWrite = new BinaryWriter(new MemoryStream(value));
            binWrite.Write((byte)asicType);
            binWrite.Write((byte)deviceType);
            binWrite.Write((byte)deviceIndex);
            foreach (TEraseItem erItem in eraseList)
            {
                erItem.Write(binWrite);
            }
            binWrite.Close();
            base.Write(bw);
        }

        public TEraseItem GetEraseItemForAddr(UInt32 addr)
        {
            foreach(TEraseItem erItem in eraseList)
                if (erItem.ContainAddr(addr))
                    return erItem;
            return null;
        }

        public void ChangeEraseItemForAddr(UInt32 oldStartLocation, Int32 startOffset, Int32 endOffset)
        {
            // Usa OldStartLocation per trovare l'area che la contiene...
            TEraseItem erItem = GetEraseItemForAddr(oldStartLocation);
            if (erItem == null)
                throw new FwException("Erase Area not Found for: " + BytesUtils.ToHex(oldStartLocation));
            Int64 newStartLocation = erItem.start + startOffset;
            Int64 newEndLocation = erItem.end + endOffset;
            erItem.start = (UInt32)newStartLocation;
            erItem.end = (UInt32)newEndLocation;
        }

        public double TotAreaErase()
        {
            double totSize = 0;
            foreach (TEraseItem erItem in eraseList)
            {
                totSize += erItem.Size;
            }
            return totSize;
        }

        public override string ToString()
        {
            string s =  base.ToString() + Environment.NewLine + "\tDeviceType: "+deviceType + " AsicType:" + asicType;
            int i = 0;
            foreach (TEraseItem erItem in eraseList)
            {
                if (s.Length > 0)
                    s += Environment.NewLine;
                s += "\t" + i + ":" + erItem.ToString();
                i++;
            }
            return s;
        }
    }


    public enum TPartitionId
    {
        NBL1 = 0,
        NBL2 = 1,
        NBL3 = 2,
        COPIEDOS = 3,      
        DEMANDONOS = 4,    
        PARAM = 5,
        UPDATEUTIL = 6,
        FILESYSTEM = 8,
        FILESYSTEM1 = 9,   
        PMM = 0xA,
    }


    public class TPartitionMuxOneNAND : TPartition
    {
        public TPartitionId partId;
        public UInt32 attr;     // 0x2 = RO   0x1 = RW   0x20 = FROZEN
        public UInt32 partAddr;
        public UInt32 partSize;

        public UInt32 EndAddr
        {
            get
            {
                return partAddr + partSize;
            }
        }

        public override bool ContainAddr(uint addr)
        {
            return ((addr >= partAddr) && (addr <= EndAddr));
        }

        public TPartitionMuxOneNAND(BinaryReader binRead)
        {
            // Mux
            partId = (TPartitionId)BytesUtils.SwapBytes(binRead.ReadUInt32());
            attr = BytesUtils.SwapBytes(binRead.ReadUInt32());  // 2 = RO   1 = RW
            partAddr = BytesUtils.SwapBytes(binRead.ReadUInt32());
            partSize = BytesUtils.SwapBytes(binRead.ReadUInt32());
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(BytesUtils.SwapBytes((UInt32)partId));
            bw.Write(BytesUtils.SwapBytes(attr));
            bw.Write(BytesUtils.SwapBytes(partAddr));
            bw.Write(BytesUtils.SwapBytes(partSize));
        }

        public override void ChangeStart(UInt32 startAddr)
        {
            UInt32 constEnd = partAddr + partSize;
            if (startAddr >= constEnd)
                throw new FwException("Underflow in Partition Start, Please Contact: m.bellino@symbian-toys.com");

            switch (partId)
            {
                case TPartitionId.FILESYSTEM:
                    partAddr = startAddr;
                    partSize = constEnd - partAddr;
                    break;
                default:
                    throw new FwException("This Partition Start Can't be Changed, Please Contact: m.bellino@symbian-toys.com");
            }
        }
        public override void ChangeEnd(UInt32 endAddr)
        {
            if (endAddr <= partAddr)
                throw new FwException("Underflow in Partition End, Please Contact: m.bellino@symbian-toys.com");
            switch (partId)
            {
                case TPartitionId.DEMANDONOS:
                    partSize = endAddr - partAddr;
                    break;
                default:
                    throw new FwException("This Partition Can't be Resized, Please Contact: m.bellino@symbian-toys.com");
            }
        }

#if DEBUG
        // We override only in release builds, so SmartAssembly will obfuscate the code.
        public override string ToString()
        {
            string attr_s = "";
            switch (attr)
            {
                case 0x1:
                    attr_s = "RW";
                    break;
                case 0x2:
                    attr_s = "RO";
                    break;
                case 0x20:
                    attr_s = "FROZEN";
                    break;
                default:
                    attr_s = "UNKN";
                    break;
            }
            return "\t" + BytesUtils.ToHex(partAddr) + " - " + BytesUtils.ToHex(EndAddr) + "  " + attr_s + "  " + partId.ToString() + "  Size:" + BytesUtils.GetSizeDescr(partSize);
//            return "\t" + partId.ToString() + " " + attr_s + "  " + BytesUtils.ToHex(partAddr) + " - " + BytesUtils.ToHex(EndAddr) + "  Size:" + partSize;
        }
#endif
    }

    public class TPartitionMMC : TPartition
    {
        public string descr;

        public TPartitionMMC(BinaryReader binRead)
        {
            // PARTITION_INFO_BB5: FF FF 00 01 FF FF FF FF 42 47 41 48 53 4D 4D 43 50 49 3A 00 00 00 02 00 00 01 00 01 00 02 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF 00 00 00 00 01 C9 A0 00 00 00 00 01 03 0B FF FF 00 00 02 20 FF FF FF FF FF FF FF FF 01 C9 A0 00 00 08 00 00 00 00 00 01 05 F8 FF FF FF FF FF FF FF FF FF FF FF FF FF FF
            byte[] tmp = binRead.ReadBytes(14);
            descr = BytesUtils.ToString(tmp);
        }

        public override bool ContainAddr(uint addr)
        {
            return false;
            //throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override void ChangeStart(UInt32 startAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override void ChangeEnd(UInt32 endAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Encoding.ASCII.GetBytes(descr));
        }
    }


    public class TPartitionNANDv0100000 : TPartition
    {
        public TPartitionId partId;
        public UInt32 partAddr;
        public UInt32 partSize;
        public UInt32 attr;     // 0x2 = RO   0x1 = RW   0x20 = FROZEN

        public UInt32 EndAddr
        {
            get
            {
                return partAddr + partSize;
            }
        }

        public TPartitionNANDv0100000(BinaryReader binRead)
        {
            /* NAND: N95   vers 0x0100000
              ID              : 00000003  COPIEDOS 
              Attribute       : 00000002  RO 
              Start Address   : 00A40000
              Size            : 02AC0000
            */
            partId = (TPartitionId)BytesUtils.SwapBytes(binRead.ReadUInt32());
            attr = BytesUtils.SwapBytes(binRead.ReadUInt32());
            partAddr = BytesUtils.SwapBytes(binRead.ReadUInt32());
            partSize = BytesUtils.SwapBytes(binRead.ReadUInt32());
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(BytesUtils.SwapBytes((UInt32)partId));
            bw.Write(BytesUtils.SwapBytes(attr));
            bw.Write(BytesUtils.SwapBytes(partAddr));
            bw.Write(BytesUtils.SwapBytes(partSize));
        }

        public override void ChangeStart(UInt32 startAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override void ChangeEnd(UInt32 endAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override bool ContainAddr(uint addr)
        {
            return ((addr >= partAddr) && (addr <= EndAddr));
        }

#if DEBUG
        // We override only in release builds, so SmartAssembly will obfuscate the code.
        public override string ToString()
        {
            string attr_s = "";
            switch (attr)
            {
                case 0x1:
                    attr_s = "RW";
                    break;
                case 0x2:
                    attr_s = "RO";
                    break;
                case 0x20:
                    attr_s = "FROZEN";
                    break;
                default:
                    attr_s = "UNKN";
                    break;
            }
            return "\t" + BytesUtils.ToHex(partAddr) + " - " + BytesUtils.ToHex(EndAddr) + "  " + attr_s + "  " + partId.ToString() + "  Size:" + BytesUtils.GetSizeDescr(partSize);
        }
#endif    
    }


    public class TPartitionNAND : TPartition
    {
        public TPartitionId partId;
        public UInt16 const0;
        public UInt32 partAddr;
        public UInt32 const0_b;
        public UInt32 partSize;
        public byte[] unkn;
        public UInt32 attr;     // 0x2 = RO   0x1 = RW   0x20 = FROZEN
        public string descr;

        public UInt32 EndAddr
        {
            get
            {
                return partAddr + partSize;
            }
        }

        public TPartitionNAND(BinaryReader binRead)
        {
            // NAND: N96
            /*
        FILESYSTEM1	00 00 00 09 00 00 00 00 00 00 00 00 00 00 00 00 0A BA 00 00 00 00 00 00 00 2A 00 00 52 4F 41 52 45 41 00 00 00 00 00 00 00 00	ROAREA
        COPIEDOS	00 00 00 03 00 00 00 00 02 E4 00 00 00 00 00 00 04 94 00 00 00 00 00 00 00 00 00 00 52 4F 46 53 50 52 49 4D 41 52 59 00 00 00 	ROFSPRIMARY
        COPIEDOS	00 00 00 03 00 00 00 00 07 78 00 00 00 00 00 00 02 28 00 00 00 00 00 00 00 00 00 00 52 4F 46 53 50 52 49 4D 41 52 59 00 00 00 	ROFSPRIMARY
        COPIEDOS	00 00 00 03 00 00 00 00 09 A0 00 00 00 00 00 00 00 F0 00 00 00 00 00 00 00 00 00 00 52 4F 46 53 50 52 49 4D 41 52 59 00 00 00   ROFSPRIMARY
        DEMANDONOS	00 00 00 04 00 00 00 00 0A BA 00 00 00 00 00 00 05 3E 00 00 00 00 00 00 00 28 00 00 55 44 41 31 00 00 00 00 00 00 00 00 00 00   UDA1
            */
            partId = (TPartitionId)BytesUtils.SwapBytes(binRead.ReadUInt32());
            const0 = BytesUtils.SwapBytes(binRead.ReadUInt16());
            partAddr = BytesUtils.SwapBytes(binRead.ReadUInt32());       // probabilmente sono blocchi. 
            const0_b = BytesUtils.SwapBytes(binRead.ReadUInt32());
            partSize = BytesUtils.SwapBytes(binRead.ReadUInt32());
            unkn = binRead.ReadBytes(6);
            attr = BytesUtils.SwapBytes(binRead.ReadUInt32());
            byte[] tmp = binRead.ReadBytes(14);
            descr = BytesUtils.ToString(tmp);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(BytesUtils.SwapBytes((UInt32)partId));
            bw.Write(BytesUtils.SwapBytes(const0));
            bw.Write(BytesUtils.SwapBytes(partAddr));
            bw.Write(BytesUtils.SwapBytes(const0_b));
            bw.Write(BytesUtils.SwapBytes(partSize));
            bw.Write(unkn);
            bw.Write(BytesUtils.SwapBytes(attr));
            bw.Write(Encoding.ASCII.GetBytes(descr));
        }

        public override void ChangeStart(UInt32 startAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override void ChangeEnd(UInt32 endAddr)
        {
            throw new FwException("This partition is not supported yet, Please Contact: m.bellino@symbian-toys.com");
        }

        public override bool ContainAddr(uint addr)
        {
            return ((addr >= partAddr) && (addr <= EndAddr));
        }

#if DEBUG
        // We override only in release builds, so SmartAssembly will obfuscate the code.
        public override string ToString()
        {
            string attr_s = "";
            switch (attr)
            {
                case 0x1:
                    attr_s = "RW";
                    break;
                case 0x2:
                    attr_s = "RO";
                    break;
                case 0x20:
                    attr_s = "FROZEN";
                    break;
                default:
                    attr_s = "UNKN";
                    break;
            }
            return "\t" + BytesUtils.ToHex(partAddr) + " - " + BytesUtils.ToHex(EndAddr) + "  " + attr_s + "  " + partId.ToString() + "  Size:" + BytesUtils.GetSizeDescr(partSize);
        }
#endif

    }

    public abstract class TPartition
    {
        // Queste info non ci sono nell'header di ROFS2 e ROFS3, quindi devo usare ERASE
        public static TPartition Factory(TDeviceType aDevType, UInt32 partVers, BinaryReader binRead)
        {
            switch (aDevType)
            {
                case TDeviceType.MuxOneNAND:
                    {
                        return new TPartitionMuxOneNAND(binRead);
                    }
                case TDeviceType.MMC:
                    {
                        return new TPartitionMMC(binRead);
                    }
                case TDeviceType.NAND:
                    {
                        if (partVers == 0x0100000)
                            return new TPartitionNANDv0100000(binRead);
                        return new TPartitionNAND(binRead);
                    }
                default:
                    throw new FwException("Partition Type not supported, Please Contact: m.bellino@symbian-toys.com");
            }
        }

        public abstract bool ContainAddr(UInt32 addr);
        public abstract void ChangeStart(UInt32 startAddr);
        public abstract void ChangeEnd(UInt32 endAddr);
        public abstract void Write(BinaryWriter bw);
    }


    public class TLV_Partition_Info_BB5 : TLV
    {
        public TASICType asicType;           // 0x00 CMT
        public TDeviceType deviceType;       // MuxOneNAND
        public byte deviceIndex;
        public List<TPartition> partitions = new List<TPartition>();
        private UInt32 partVers;
        private UInt32 spare;
        private UInt32 qtaParts;

        public TLV_Partition_Info_BB5(BinaryReader br)
            : base(TTLVType.PARTITION_INFO_BB5, br)
        {
            BinaryReader binRead = new BinaryReader(new MemoryStream(value));
            asicType = (TASICType)binRead.ReadByte();                           // APE
            deviceType = (TDeviceType)binRead.ReadByte();                       // NAND
            deviceIndex = binRead.ReadByte();                                   // 0

            // TODO: Gestisci questo tipo... RM-596_M004.44.emmc.fpsx
            if (deviceType != TDeviceType.MMC)
            {
                partVers = BytesUtils.SwapBytes(binRead.ReadUInt32());          // 0xffff0000   0x00010000
                spare = binRead.ReadUInt32();                                   // 0xffffffff
                qtaParts = BytesUtils.SwapBytes(binRead.ReadUInt32());

                for (int i = 0; i < qtaParts; i++)
                {
                    TPartition part = TPartition.Factory(deviceType, partVers, binRead);
                    partitions.Add(part);
                }
            }
            binRead.Close();
        }

        public TPartition GetPartitionForAddr(UInt32 address)
        {
            foreach (TPartition part in partitions)
                if (part.ContainAddr(address))
                    return part;
            return null;
        }

        public override void Write(BinaryWriter bw)
        {
            // TODO: Gestisci questo tipo... RM-596_M004.44.emmc.fpsx 
            if (deviceType == TDeviceType.MMC)
            {
                base.Write(bw);
                return;
            }
            BinaryWriter binWrite = new BinaryWriter(new MemoryStream(value));
            binWrite.Write((byte)asicType);
            binWrite.Write((byte)deviceType);
            binWrite.Write((byte)deviceIndex);
            binWrite.Write((UInt32)BytesUtils.SwapBytes(partVers));
            binWrite.Write((UInt32)BytesUtils.SwapBytes(spare));
            binWrite.Write((UInt32)BytesUtils.SwapBytes(qtaParts));
            foreach (TPartition aPart in partitions)
            {
                aPart.Write(binWrite);
            }
            binWrite.Close();
            base.Write(bw);
        }

        private void GetSystemAndUserParts(out TPartitionMuxOneNAND rofsPart, out TPartitionMuxOneNAND fatPart)
        {
            rofsPart = null;
            fatPart = null;
            if (deviceType != TDeviceType.MuxOneNAND)
                throw new FwException("Partition Type not supported, Please Contact: m.bellino@symbian-toys.com");
            foreach (TPartition aPart in partitions)
            {
                TPartitionMuxOneNAND muxPart = aPart as TPartitionMuxOneNAND;
                if (muxPart != null && muxPart.partId == TPartitionId.DEMANDONOS)
                {
                    if (rofsPart != null)
                        throw new FwException("Multiple ROFS Partitions, Please Contact: m.bellino@symbian-toys.com");
                    rofsPart = muxPart;
                }
                if (muxPart != null && muxPart.partId == TPartitionId.FILESYSTEM)
                {
                    if (fatPart != null)
                        throw new FwException("Multiple FAT Partitions, Please Contact: m.bellino@symbian-toys.com");
                    fatPart = muxPart;
                }
            }
            if (rofsPart == null || fatPart == null)
                throw new FwException("Can't Detect FAT and ROFS Partitions, Please Contact: m.bellino@symbian-toys.com");
        }

        /* not needed
        public void GetSystemAndUserValues(out UInt32 startRofs, out UInt32 endRofs, out UInt32 startFat, out UInt32 endFat)
        {
            // Valido solo per il CORE.
            TPartitionMuxOneNAND rofsPart;
            TPartitionMuxOneNAND fatPart;
            GetSystemAndUserParts(out rofsPart, out fatPart);
            startRofs = rofsPart.partAddr;
            endRofs = rofsPart.EndAddr;
            startFat = fatPart.partAddr;
            endFat = fatPart.EndAddr;
        }*/

        public void ChangeSystemEnd(UInt32 rofs123Len)
        {
            // Valido solo per il CORE
            TPartitionMuxOneNAND rofs;
            TPartitionMuxOneNAND fat;
            GetSystemAndUserParts(out rofs, out fat);

            UInt32 newEndRofs = rofs.partAddr + rofs123Len;
            if (fat.partAddr < rofs.EndAddr)
                throw new FwException("Underflow in Gap, Please Contact: m.bellino@symbian-toys.com");
            UInt32 gap = fat.partAddr - rofs.EndAddr;
            if (newEndRofs >= fat.EndAddr - gap)
                throw new FwException("New ROFS Size is too big, Please Contact: m.bellino@symbian-toys.com");
            if (newEndRofs <= rofs.partAddr)
                throw new FwException("New ROFS Size is too small, Please Contact: m.bellino@symbian-toys.com");
            rofs.ChangeEnd(newEndRofs);
            fat.ChangeStart(newEndRofs+gap);
        }

#if DEBUG
        public override string ToString()
        {
            string descr = "";
            foreach (TPartition part in partitions)
            {
                if (descr.Length > 0)
                    descr += Environment.NewLine;
                descr += part;
            }
            UInt64 totSize = 0;
            foreach (TPartition part in partitions)
            {
                TPartitionMuxOneNAND partMux = part as TPartitionMuxOneNAND;
                if (partMux != null)
                    totSize += partMux.partSize;
                TPartitionNAND partNand = part as TPartitionNAND;
                if (partNand != null)
                    totSize += partNand.partSize;
            }
            return base.ToString() + Environment.NewLine + deviceType + "  Size: "+ BytesUtils.GetSizeDescr(totSize) + Environment.NewLine + descr;
        }
#endif
    }/**/


#if DEBUG
    // Formattazione della partizione PPM
    public class deb_TLV_Format_Partition_BB5 : TLV
    {
        public TASICType asicType;              // 0x00 CMT
        public TDeviceType deviceType;          // MuxOneNAND
        public byte deviceIndex;
        private string _descr = "";

        public deb_TLV_Format_Partition_BB5(BinaryReader br)
            : base(TTLVType.FORMAT_PARTITION_BB5, br)
        {
            asicType = (TASICType)value[0];
            deviceType = (TDeviceType)value[1];
            deviceIndex = value[2];
            int infoLen = (value.Length - 3 - 4);
            // TODO:
            // Format Info ID   : 0x03000000
            // Format Info      : ....
        }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + _descr;
        }
    }/**/
#endif

}