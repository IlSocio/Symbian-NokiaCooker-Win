using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FuzzyByte.Utils;


namespace Firmware
{
    [Serializable]
    public abstract class TBlockHeader
    {
        public TASICType flashMemory;           
        public UInt32 contentLength = 0;
        public UInt16 contentChecksum16 = 0;
        public UInt32 location = 0;             // page?

        public UInt32 FirstLocation
        {
            get
            {
                return location;
            }
        }

        public UInt32 LastLocation
        {
            get
            {
                return location + contentLength;
            }
        }

        /*        public abstract bool IsValidChecksum(BinaryReader br, byte[] content);
                public abstract void FixChecksum(BinaryReader br, byte[] content);*/

        public abstract void Read(BinaryReader br);

        public abstract void Write(BinaryWriter bw);


        public static TBlockHeader Factory(BinaryReader br, TBlockType blockType)
        {
            switch (blockType)
            {
                // BlockType Code... oppure Unknown
                case TBlockType.BlockType17:
                    return new TBlockHeader17();
                case TBlockType.BlockType2E:
                    return new TBlockHeader2E();
                case TBlockType.BlockType30:
                    return new TBlockHeader30();
                case TBlockType.BlockType49:
                    return new TBlockHeader49();

                // BlockType DataCert
                case TBlockType.BlockType27_ROFS_Hash:
                    return new TBlockHeader27_ROFS_Hash();
                case TBlockType.BlockType28_CORE_Cert:
                    return new TBlockHeader28_CORE_Cert();

//#if DEBUG
                case TBlockType.BlockType3A:
                    return new TBlockHeader3A();
//#endif
                default:
                    throw new FwException("Unknown block, Please Contact: m.bellino@symbian-toys.com");
            }

            /*
            switch (blockType)
            {
                case TContentType.Code:
                    {
                        switch (contentType)
                        {
                            case TBlockType.BlockType17:
                                return new TBlockType17();
                            case TBlockType.BlockType2E:
                                return new TBlockType2E();
                            case TBlockType.BlockType30:
                                return new TBlockType30();
                            case TBlockType.BlockType49:
                                return new TBlockType49();
                        }
                        throw new FwException("Unknown Code Content, Please Contact: m.bellino@symbian-toys.com");
                    }
                case TContentType.DataCert:
                    {
                        switch (contentType)
                        {
                            case TBlockType.BlockType27_ROFS_Hash:
                                return new TBlockType27_ROFS_Hash();
                            case TBlockType.BlockType28_CORE_Cert:
                                return new TBlockType28_CORE_Cert();
                        }
                        throw new FwException("Unknown Data Content, Please Contact: m.bellino@symbian-toys.com");
                    }
            }
            */
        }

        public override string ToString()
        {
            return flashMemory.ToString();
        }
    }


    [Serializable]
    class TBlockHeader49 : TBlockHeader
    {
        //public UInt16 unkn2;         // N95 = 0x0001   5800 = 0x0003    N8 = 0x0004 (Forse in relazione con il tipo di partizione)
        //public byte const01;

        public override void Read(BinaryReader br)
        {
            contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            contentChecksum16 = br.ReadUInt16();                // TODO: Crc32
            byte[] trash1 = br.ReadBytes(0x0A);
            //contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            byte[] trash2 = br.ReadBytes(0x08);
            location = BytesUtils.SwapBytes(br.ReadUInt32());
            if (contentChecksum16 != 0)
            {
            }
            Debug.WriteLine("CRC: " + BytesUtils.ToHex(contentChecksum16));
            Debug.WriteLine("Trash1: " + BytesUtils.ToHex(trash1));
            Debug.WriteLine("Trash2: " + BytesUtils.ToHex(trash2));
        }

        public override void Write(BinaryWriter bw)
        {
            throw new FwException("The repack of this fw is not supported, Please Contact: m.bellino@symbian-toys.com");
        }
    }

    [Serializable]
    class TBlockHeader3A : TBlockHeader
    {
        public string description;      // 12
        public UInt16 unkn2;

        public override void Read(BinaryReader br)
        {
            flashMemory = (TASICType)br.ReadByte();
            unkn2 = br.ReadUInt16();
            if (unkn2 != 0x1c08)
                throw new FwException("unkn2 != 1c08, Please Contact: m.bellino@symbian-toys.com");

            contentChecksum16 = br.ReadUInt16();
            contentLength = 0;
            byte[] descrBytes = br.ReadBytes(12);
            description = ASCIIEncoding.ASCII.GetString(descrBytes);
        }
        public override void Write(BinaryWriter bw)
        {
/*            bw.Write((byte)flashMemory);
            bw.Write(unkn2);
            bw.Write(contentChecksum16);
            bw.Write(const01);
            bw.Write(BytesUtils.SwapBytes(contentLength));
            bw.Write(BytesUtils.SwapBytes(location));*/
        }
    }

    [Serializable]
    class TBlockHeader17 : TBlockHeader
    {
        //public byte processorType;
        public UInt16 unkn2;         // N95 = 0x0001   5800 = 0x0003        
        //public UInt16 contentCrc16;
        public byte const01;
        // size
        // public UInt32 location;

        public override void Read(BinaryReader br)
        {
            flashMemory = (TASICType)br.ReadByte();
            unkn2 = br.ReadUInt16();
            // Forse unkn2 e' TASICType + TDeviceType

            contentChecksum16 = br.ReadUInt16();
            const01 = br.ReadByte();
            if (const01 != 01)
                throw new FwException("Block17 Const != 01, Please Contact: m.bellino@symbian-toys.com");

            contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            location = BytesUtils.SwapBytes(br.ReadUInt32());
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)flashMemory);
            bw.Write(unkn2);
            bw.Write(contentChecksum16);
            bw.Write(const01);
            bw.Write(BytesUtils.SwapBytes(contentLength));
            bw.Write(BytesUtils.SwapBytes(location));
        }
    }

    [Serializable]
    class TBlockHeader2E : TBlockHeader       
    {
        //public byte processorType;
        public UInt16 unkn2;
        // public UInt16 contentCrc16;
        public string description;      // 12
        // size
        // public UInt32 location;

        public override void Read(BinaryReader br)
        {
            flashMemory = (TASICType)br.ReadByte();
            unkn2 = br.ReadUInt16();
            contentChecksum16 = br.ReadUInt16();

            byte[] descrBytes = br.ReadBytes(12);
            description = ASCIIEncoding.ASCII.GetString(descrBytes);

            contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            location = BytesUtils.SwapBytes(br.ReadUInt32());
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)flashMemory);
            bw.Write(unkn2);
            bw.Write(contentChecksum16);
            byte[] descrBytes = ASCIIEncoding.ASCII.GetBytes(description);
            bw.Write(descrBytes);
            bw.Write(BytesUtils.SwapBytes(contentLength));
            bw.Write(BytesUtils.SwapBytes(location));
        }
    }

    [Serializable]
    class TBlockHeader30 : TBlockHeader       
    {
        //public byte processorType;
        public UInt16 unkn2;
        // public UInt16 contentCrc16;
        public byte[] unkn8;            // 8 bytes
        // size
        // public UInt32 location;

        public override void Read(BinaryReader br)
        {
            flashMemory = (TASICType)br.ReadByte();
            unkn2 = br.ReadUInt16();
            contentChecksum16 = br.ReadUInt16();

            unkn8 = br.ReadBytes(8);
            contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            location = BytesUtils.SwapBytes(br.ReadUInt32());
        }


        // TODO: Leggi la FAT contenuta in RM-472_30.033_100.1_prd.uda.fpsx
        // TODO: Perche' sembra che scazza a fare il salvataggio di questi dati...


        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)flashMemory);
            bw.Write(unkn2);
            bw.Write(contentChecksum16);
            bw.Write(unkn8);
            bw.Write(BytesUtils.SwapBytes(contentLength));
            bw.Write(BytesUtils.SwapBytes(location));
        }
    }

    
    [Serializable]
    public class TBlockHeader27_ROFS_Hash : TBlockHeader
    {
        public byte[] cmt_root_key_hash20_sha1; // 20       CMT_ROOT_KEY_HASH: 916F75217F32081248B15C38DFC8E81B (corrisponde a SHA1 dei contenuti solo per la sezione blocco KEYS) gli altri blocchi sono linkati a questo.
        public string description;              // 12
        // public byte processorType;
        public UInt16 unkn2;
        // public UInt16 contentCrc16;       
        // size
        // public UInt32 location;

        public override void Read(BinaryReader br)
        {
            cmt_root_key_hash20_sha1 = br.ReadBytes(20);
            //unkn4 = br.ReadBytes(4);
            byte[] descrBytes = br.ReadBytes(12);
            description = ASCIIEncoding.ASCII.GetString(descrBytes);
            flashMemory = (TASICType)br.ReadByte();
            unkn2 = br.ReadUInt16();
            contentChecksum16 = br.ReadUInt16();
            contentLength = BytesUtils.SwapBytes(br.ReadUInt32());
            location = BytesUtils.SwapBytes(br.ReadUInt32());
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(cmt_root_key_hash20_sha1);
            byte[] descrBytes = ASCIIEncoding.ASCII.GetBytes(description);
            bw.Write(descrBytes);
            bw.Write((byte)flashMemory);
            bw.Write(unkn2);
            bw.Write(contentChecksum16);
            bw.Write(BytesUtils.SwapBytes(contentLength));
            bw.Write(BytesUtils.SwapBytes(location));
        }

        public override string ToString()
        {
            return base.ToString() + "\t" + base.flashMemory + "_ROOT_KEY: " + BytesUtils.ToHex(cmt_root_key_hash20_sha1).Replace(" ", "");
        }
    }

    [Serializable]
    class TBlockHeader28_CORE_Cert : TBlockHeader27_ROFS_Hash
    {
        public byte[] rap_papub_keys_maybe_hash20_sha1;     // RAP_PAPUB_KEYS 20 bytes   (Maybe it is a SHA1-RSA Digital Signature?)  
                                                            // Will not be saved in the ROFS, but will match the one already contained in the ROFS.
        public byte[] unkn2b;

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            rap_papub_keys_maybe_hash20_sha1 = br.ReadBytes(20);
            unkn2b = br.ReadBytes(2);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(rap_papub_keys_maybe_hash20_sha1);
            bw.Write(unkn2b);
        }

        public override string ToString()
        {
            return base.ToString() + "\tRAP_PAPUB_KEY: " + BytesUtils.ToHex(rap_papub_keys_maybe_hash20_sha1).Replace(" ", "");
        }
    }
}
