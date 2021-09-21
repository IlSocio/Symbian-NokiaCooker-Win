using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;
using System.Diagnostics;

namespace Firmware
{
    /*
    #define     PARTITION_ID_NBL			    0		///< NAND bootloader stage 1, 2 
    #define     PARTITION_ID_BOOTLOADER		    1		///< NAND bootloader stage 3
    #define     PARTITION_ID_BOOT_PARAMETER		2		///< NAND bootloader parameter of stage 3
    */

    public enum TDeviceType
    {
        NOR = 0,
        NAND = 1,
        MuxOneNAND = 3,
        MMC = 4,
        RAM = 5
    }

    public enum TASICType   // ProcessorType
    {
        CMT = 0,        // ROFS
        APE = 1,        // CORE
        BOTH = 2
    }

    /*
    public enum EnumFlashingFileSubType
    {
        eFlashingFileNoSubType = 0,
        eFlashingFileSubTypeContent = 3,
        eFlashingFileSubTypeInDeviceOnly = 6,
        eFlashingFileSubTypeMcu = 1,
        eFlashingFileSubTypeMemoryCardContent = 4,
        eFlashingFileSubTypeMmcErase = 8,
        eFlashingFileSubTypePpm = 2,
        eFlashingFileSubTypeSymbianTest = 5,
        eFlashingFileSubTypeUdaErase = 7
    }
    public enum EnumCertificateTypes
    {
        eCertificateTypesCcc,       // IMPORTANTE... CccId
        eCertificateTypesHwc,
        eCertificateTypesNpc,
        eCertificateTypesRd,
        eCertificateTypesVariant,
        eCertificateTypesMid,
        eCertificateTypesIfxRd,
        eCertificateTypesSteMDM,
        eCertificateTypesPartnerC
    }
    /**/

    public enum TContentType
    {
        Code = 0x54,
        Unkn1 = 0x56,
        DataCert = 0x5D
    }

    public enum TBlockType
    {
        BlockType17 = 0x17,             // Code o Unknown   (In generale, puo' esserci qualsiasi ContentType)
        BlockType2E = 0x2E,             // Code
        BlockType30 = 0x30,             // Code
        BlockType3A = 0x3A,             // Unknown
        BlockType27_ROFS_Hash = 0x27,   // Data
        BlockType28_CORE_Cert = 0x28,   // Data
        BlockType49 = 0x49              // UDA_N8_LargeFile
    }

    // EntryPoint... PageFormat... MaxPage...
    public enum TTLVType
    {
        MORE_RootKeyHash_MORE = 0x0D,
        ERASE_AREA_BB5 = 0x12,
        ONENAND_SubType_Unkn2 = 0x13,
        FORMAT_PARTITION_BB5 = 0x19,
        PARTITION_INFO_BB5 = 0x2F,
        CMT_Type = 0xC2,
        CMT_Algo = 0xC3,
        ERASE_DCT5 = 0xC8,
        UnknC9 = 0xC9,
        SecondarySendingSpeed = 0xCD,
        AlgoSendingSpeed = 0xCE,
        ProgramSendingSpeed = 0xCF,
        MessageReadingSpeed = 0xD1,
        CMT_SupportedHW = 0xD4,
        APE_SupportedHW = 0xE1,
        DateTime = 0xE6,
        APE_Phone_Type = 0xE7,
        APE_Algorithm = 0xE8,
        UnknEC = 0xEC,
        UnknED = 0xED,
        UnknE4_Imp = 0xE4,
        UnknE5 = 0xE5,
        UnknEA = 0xEA,
        Array = 0xEE,
        UnknF3_Imp = 0xF3,              // Sono dei THeader
        Descr = 0xF4,
        UnknF6 = 0xF6,
        UnknF7 = 0xF7,
        UnknFA_Imp = 0xFA               // Sono dei THeader
    }


    //Reading SHA1-RSA Signature...
    //Reading SHA1-HMAC Signature...

    public class THeader
    {
        public Int32 offsetFixCrc = 0;
        public byte signature;
        public UInt32 headerSize;
        public byte[] unkn = new byte[0];
        public List<TLV> tlvData = new List<TLV>();

        public void Clear()
        {
            offsetFixCrc = 0;
            signature = 0;
            headerSize = 0;
            unkn = new byte[0];
            tlvData.Clear();
        }

        /*
    ERASE_DCT5: 02 12 1B 00 03 00 00 04 00 00 00 07 FF FF 00 0C 00 00 00 8B FF FF 00 DC 00 00 0E 97 FF FF 19 2F 00 03 00 00 00 00 03 00 00 00 00 00 00 00 0A 00 00 00 00 00 00 00 02 00 00 00 64 00 18 00 00 00 00 00 02 00 00 00 01 00 00 00 00 00 00 00 00
	ERASE_AREA_BB5: 00 03 00 00 04 00 00 00 07 FF FF 00 0C 00 00 00 8B FF FF 00 DC 00 00 0E 97 FF FF
	0:00040000-0007FFFF  Size: 256 Kb	MuxOneNAND CMT
	1:000C0000-008BFFFF  Size: 8 Mb	MuxOneNAND CMT
	2:00DC0000-0E97FFFF  Size: 219,75 Mb	MuxOneNAND CMT				 **0E97FFFF**
	Tot Erase Area: 228 Mb
         */
        private TLV_Erase_Area_BB5 GetEraseAreaForAddr(UInt32 addr)
        {
            foreach (TLV tlv in tlvData)
            {
                TLV_Erase_DCT5 tlvEr = tlv as TLV_Erase_DCT5;
                if (tlvEr == null)
                    continue;
                // N95 ha piu' di una Erase Area BB5..
                foreach (TLV aTlvOper in tlvEr.operations)
                {
                    TLV_Erase_Area_BB5 tlvBb5 = aTlvOper as TLV_Erase_Area_BB5;
                    if (tlvBb5 != null && tlvBb5.GetEraseItemForAddr(addr) != null)
                        return tlvBb5;
                }
            }
            return null;
        }

        public void ChangeEraseItemForAddr(UInt32 oldStartLocation, Int32 startOffset, Int32 endOffset)
        {
            TLV_Erase_Area_BB5 area = GetEraseAreaForAddr(oldStartLocation);
            if (area == null)
                throw new FwException("Can't find Erase Area for: "+BytesUtils.ToHex(oldStartLocation));
            area.ChangeEraseItemForAddr(oldStartLocation, startOffset, endOffset);
        }

        public bool GetEraseLimitsForAddr(UInt32 addr, out UInt32 lowLimit, out UInt32 upperLimit)
        {
            lowLimit = 0;
            upperLimit = 0;
            TLV_Erase_Area_BB5 erArea = GetEraseAreaForAddr(addr);
            if (erArea == null)
                return false;
            TEraseItem item = erArea.GetEraseItemForAddr(addr);
            if (item == null)
                return false;

            lowLimit = item.start;
            upperLimit = item.end;
            return true;
        }

        /*
    PARTITION_INFO_BB5: 00 03 00 00 02 00 00 FF FF FF FF 00 00 00 04 00 00 00 03 00 00 00 02 00 14 00 00 02 A4 00 00 00 00 00 04 00 00 00 02 02 B8 00 00 0B E0 00 00 00 00 00 08 00 00 00 01 0E A0 00 00 10 60 00 00 00 00 00 0A 00 00 00 01 1F 00 00 00 00 40 00 00
	COPIEDOS   2 Start:00140000 End:02B80000 Size:44302336
	DEMANDONOS 2 Start:02B80000 End:0E980000 Size:199229440				 **0E980000**
	FILESYSTEM 1 Start:0EA00000 End:1F000000 Size:274726912			**0EA00000**
	PMM        1 Start:1F000000 End:1F400000 Size:4194304
        */
        public TLV_Partition_Info_BB5 GetPartitionArea()
        {
            foreach (TLV tlv in tlvData)
            {
                TLV_EE_Array arr = tlv as TLV_EE_Array;
                if (arr == null)
                    continue;
                foreach (TLV newTlv in arr.array)
                {
                    TLV_Partition_Info_BB5 bb5Tlv = newTlv as TLV_Partition_Info_BB5;
                    if (bb5Tlv != null)
                        return bb5Tlv;
                }
            }
            return null;
        }

        public bool GetPartitionLimitsForAddr(UInt32 addr, out UInt32 lowLimit, out UInt32 upperLimit)
        {
            lowLimit = 0;
            upperLimit = 0;
            TLV_Partition_Info_BB5 partArea = GetPartitionArea();
            if (partArea == null)
                return false;
            TPartition part = partArea.GetPartitionForAddr(addr);
            if (part == null)
                return false;

            TPartitionMuxOneNAND partMux = part as TPartitionMuxOneNAND;
            if (partMux != null)
            {
                lowLimit = partMux.partAddr;
                upperLimit = partMux.EndAddr;
                return true;
            }

            TPartitionNAND partNand = part as TPartitionNAND;
            if (partMux != null)
            {
                lowLimit = partNand.partAddr;
                upperLimit = partNand.EndAddr;
                return true;
            }
            return true;
        }



        /*        public UInt32 GetPartitionSize(UInt32 firstLocation, UInt32 lastLocation)
                {
                    foreach (TLV tlv in tlvData)
                    {
                        TLV_Erase_DCT5 tlvEr = tlv as TLV_Erase_DCT5;
                        if (tlvEr != null && tlvEr.operations.Count > 0)
                        {
                            TLV_Erase_Area_BB5 tlvBb5 = tlvEr.operations[0] as TLV_Erase_Area_BB5;
                            if (tlvBb5 != null && tlvBb5.startAddr.Count > 0)
                            {
                                double size = (tlvBb5.endAddr[0] - tlvBb5.startAddr[0]);
                                return (UInt32)size;
                            }
                        }
                    }
                    return 0;
                }*/


        /*        private UInt32 PullPiece(UInt32 buffer, int startbit, int endbit)
                 {
                     UInt32 retval = 0;
                     BitArray ender = new BitArray(32);
                     BitArray ba = new BitArray(new Int32[] { (Int32)buffer });
                     int counter=0;
                     for (int i = startbit; i <= endbit; i++)
                     {
                         ender[counter] = ba[i];
                         counter++;
                     }
                     byte[] interm = new byte[4];
                     ender.CopyTo(interm, 0);
                     retval = BitConverter.ToUInt32(interm, 0);
                     return retval;
                 }/**/

        public void Read(BinaryReader br)
        {
            Read(br, false);
        }
        public void Read(BinaryReader br, bool supportPartition)
        {
            signature = br.ReadByte();
            if (signature < 0xB0 || signature > 0xB2)
                throw new InvalidFwException("This is not a Firmware File!!!");

            headerSize = BytesUtils.SwapBytes(br.ReadUInt32());
            long oldPos = br.BaseStream.Position;

            Debug.WriteLine("**********************");
            Debug.WriteLine("EntryPoint: " + BytesUtils.ToHex(headerSize + 5));
            UInt32 qtaTlvData = BytesUtils.SwapBytes(br.ReadUInt32());
            for (int i = 0; i < qtaTlvData; i++)
            {
                TLV newTLV = TLV.Factory(br, supportPartition);

                Debug.WriteLine(newTLV);
                Debug.WriteLine(" -Str: " + BytesUtils.ToString(newTLV.value));
                tlvData.Add(newTLV);
                /*                if (newTLV.type == TTLVType.CMT_Type ||
                                    newTLV.type == TTLVType.CMT_Algo ||
                                    newTLV.type == TTLVType.SecondarySendingSpeed ||
                                    newTLV.type == TTLVType.AlgoSendingSpeed ||
                                    newTLV.type == TTLVType.ProgramSendingSpeed ||
                                    newTLV.type == TTLVType.MessageReadingSpeed ||
                                    newTLV.type == TTLVType.CMT_SupportedHW ||
                                    newTLV.type == TTLVType.APE_SupportedHW ||
                                    newTLV.type == TTLVType.APE_Phone_Type ||
                                    newTLV.type == TTLVType.APE_Algorithm ||
                                newTLV.type == TTLVType.APE_Phone_Type ||
                                newTLV.type == TTLVType.DateTime)
                                continue;/**/

                switch (newTLV.type)
                {
                    case TTLVType.ERASE_DCT5:
                        TLV_Erase_DCT5 er = (newTLV as TLV_Erase_DCT5);
                        if (er != null)
                        {
                            foreach (TLV oper in er.operations)
                            {
                                if (oper.type == TTLVType.FORMAT_PARTITION_BB5)
                                {

                                }
                            }
                        }
                        break;
                    case TTLVType.UnknF3_Imp:
                        break;
                    case TTLVType.UnknE5:
                        break;
                    case TTLVType.UnknFA_Imp:
                        break;
                    default:
                        break;
                }

                // UDA di N95 non ha il campo TTL Descr
                if (newTLV.type == TTLVType.APE_Phone_Type && newTLV.length >= 4 && offsetFixCrc == 0)
                {
                    offsetFixCrc = (Int32)br.BaseStream.Position - newTLV.length;
                    newTLV.value[0] = newTLV.value[1] = newTLV.value[2] = newTLV.value[3] = 0;
                }/**/

                if (newTLV.type == TTLVType.Descr && newTLV.length >= 4 && offsetFixCrc == 0)
                {
                    offsetFixCrc = (Int32)br.BaseStream.Position - newTLV.length;
                    newTLV.value[0] = newTLV.value[1] = newTLV.value[2] = newTLV.value[3] = 0;
                }
                /*
                #if DEBUG
                                if (newTLV.type == TTLVType.ERASE_DCT5)
                                {
                                    BinaryReader binRead = new BinaryReader(new MemoryStream(newTLV.value));
                                    int qtyBlocks = binRead.ReadByte();
                                    for (int j = 0; j < qtyBlocks; j++)
                                    {
                                        TLV tlv = TLV.Factory(binRead);
                                        Debug.WriteLine(tlv.ToString());
                                    }
                                    binRead.Close();
                                }
                #endif
                */
            }


            // E71 ha delle partizioni aggiuntive rm346_510.21.009_prd.c00


            long toRead = headerSize - (br.BaseStream.Position - oldPos);
            if (toRead > 0)
            {
                Debug.WriteLine("Skip Unknown Bytes...");
                throw new FwException(toRead + " Unknown bytes in header, Please Contact: m.bellino@symbian-toys.com");
            }
            unkn = new byte[toRead];
            unkn = br.ReadBytes(unkn.Length);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(signature);
            bw.Write(BytesUtils.SwapBytes(headerSize));
            UInt32 qtaData = BytesUtils.SwapBytes((UInt32)tlvData.Count);
            bw.Write(qtaData);
            foreach (TLV tlv in tlvData)
            {
                tlv.Write(bw);
            }
            bw.Write(unkn);
        }
    }
}
