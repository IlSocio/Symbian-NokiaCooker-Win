using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using FuzzyByte.Utils;
using System.Collections;
using System.Xml;



namespace Firmware
{

    public enum TFwType
    {
        UDA = 0,
        ROFS,
        ROFX,
        CORE,
        UNKNOWN
    }


    // TODO: ?Mah? Cambia Logged, le classi che la estendono e Main... in maniera che vi siano due implementazioni dello stesso delegate
    // TODO: ?Mah? La prima implementazione logga su File, la seconda logga su WinForm.

    // Non è necessario che venga estesa, basta che venga istanziata... poi pero' come aggiungo le implementazioni dei delegate?
    // Potrei fare una singleton, cosi' dal main posso aggiungere le implementazioni, FORSE.


    public class FwFile : Logged
    {
        Dictionary<string, List<TBlock>> rootKeys = new Dictionary<string, List<TBlock>>();
        public THeader header = new THeader();
        //        private static int _kkk = 0;
        public TGroupCollection blocksColl = new TGroupCollection();
        public string filename = "";
        private UInt32 _initCrc = 0;
        private Int32 _bytesToTrash = 0;
        private int _fwType = -1;
        private bool _canRemoveBlocks = true;

        public FwFile()
        {
        }

        ~FwFile()
        {
            Close();
        }

        public void Close()
        {
            rootKeys.Clear();
            header.Clear();
            blocksColl.Clear();
            filename = "";
            _initCrc = 0;
            _bytesToTrash = 0;
            _fwType = -1;
            _canRemoveBlocks = true;
        //    GC.Collect();
        }

        public void Open(string fname)
        {
            Open(fname, false);
        }

        public void Open(string fname, bool supportPartition)
        {
            /*filename = fname;
            FixCRCForWholeFile(0xD4770E11, 0x8D);/**/

            Close();
            LogEvent("Open Firmare file: " + fname + "\n", EventType.Info);

            using (BinaryReader br = new BinaryReader(new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                header.Clear();
                header.Read(br, supportPartition);
                filename = fname;
                br.Close();
            }
#if DEBUG
            if (!CanResizeUda())
            {
                string path = Path.GetDirectoryName(fname) + Path.DirectorySeparatorChar;
                foreach (TLV aTlv in header.tlvData)
                {
                    if (aTlv is TLV_F3)
                        File.WriteAllBytes(path + "_BinF3.bin", aTlv.value);
                    if (aTlv is TLV_FA)
                        File.WriteAllBytes(path + "_BinFA.bin", aTlv.value);
                }
            }
#endif
        }


        public void Read()
        {
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                br.BaseStream.Seek(header.headerSize + 5, SeekOrigin.Begin);
                /*            if (br.BaseStream.Position != header.headerSize)
                                throw new FwException("Wrong entry-point, Please contact m.bellino@symbian-toys.com");*/

                ReadAllBlocksFromFile(br);
                LogEvent("Type: " + FwTypeDescr + "\nAPE Blocks: " + this.CountBlocks(TASICType.APE) + "\nCMT Blocks: " + this.CountBlocks(TASICType.CMT) + "\nImage Length: " + (ContentLength - _bytesToTrash) + "\n");

                DateTime started = DateTime.Now;
                LogEvent("Compute original CRC for Firware file. Please wait...\t", EventType.Info);
                CRCTool crcTool = new CRCTool();
                crcTool.Init(CRCTool.CRCCode.CRC32);

                // Compute CRC using the stream
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                _initCrc = (UInt32)crcTool.crctablefast(br);
                br.Close();
                TimeSpan span = DateTime.Now - started;
                LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
                LogEvent("CRC Computed Successfully: 0x" + BytesUtils.ToHex(_initCrc) + "\n");
            }
            // byte[] allBytes = File.ReadAllBytes(fname);
            // _initCrc = (UInt32)crcTool.crcbitbybitfast(allBytes);
            //allBytes = null;

        }


        public bool CanResizeUda()
        {
            foreach (TLV aTlv in header.tlvData)
                if (aTlv is TLV_FA || aTlv is TLV_F3)
                    return false;
            return true;
        }


        private void ReadAllBlocksFromFile(BinaryReader br)
        {
            DateTime started = DateTime.Now;
            LogEvent("Read blocks from Firmware file. Please wait...\t", EventType.Info);

            blocksColl.Clear();

            //return;
            int i = 0;

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                i++;
                UInt32 blockOffset = (UInt32)br.BaseStream.Position;
                TBlock block = new TBlock();
                block.Read(br);
                block._offset = blockOffset;

#if DEBUG
                block.UpdateChecksums();
#endif
                blocksColl.AddBlock(block);

                // Code useful for reversing the file-format
                Debug.WriteLine(block);
                //LogEvent("Offset:" + BytesUtils.ToHex(blockOffset) + " Location:" + BytesUtils.ToHex(block.blockHeader.location) + " Size:" + BytesUtils.ToHex(block.blockHeader.contentLength) + " Type:" + block.blockType + " Processor:" + block.blockHeader.flashMemory + Environment.NewLine, EventType.Debug);
#if !DEBUG
                continue;
#endif
                TBlockHeader2E type2E = block.blockHeader as TBlockHeader2E;
                if (type2E != null)
                {
                    // LogEvent(" Unkn2:" + BytesUtils.ToHex(type2E.unkn2) + " Descr:" + Utils.CleanDescr(type2E.description) + Environment.NewLine, EventType.Debug);
                }

                TBlockHeader17 type17 = block.blockHeader as TBlockHeader17;
                if (type17 != null)
                {
                    //LogEvent(" Unkn2:" + BytesUtils.ToHex(type17.unkn2) + Environment.NewLine, EventType.Debug);
                }

                TBlockHeader30 type30 = block.blockHeader as TBlockHeader30;
                if (type30 != null)
                {
                    //LogEvent(" Unkn8:" + BytesUtils.ToHex(type30.unkn8) + " Unkn2:" + type30.unkn2 + Environment.NewLine, EventType.Debug);
                }

                // ...DataCert...
                //LogEvent(block.ToString() + Environment.NewLine, EventType.Debug);
                TBlockHeader28_CORE_Cert type_cert = block.blockHeader as TBlockHeader28_CORE_Cert;
                if (type_cert != null)
                {
                    //                                LogEvent("PAPUBKEYS Hash for CMT Hash20_SHA1:" + BytesUtils.ToHex(type_cert.rap_papub_keys_maybe_hash20_sha1) + " unkn2:" + BytesUtils.ToHex(type_cert.unkn2) + Environment.NewLine, EventType.Debug);
                    // Log taken from JAF: PAPUBKEYS Hash for CMT: DAEF2C397A3F8FC5CA8B1F429D73C94C4A25B15B
                }

                TBlockHeader27_ROFS_Hash type_hash = block.blockHeader as TBlockHeader27_ROFS_Hash;
                if (type_hash != null)
                {
                    //                                LogEvent("CMT_ROOT_KEY_HASH Hash16_MD5:" + BytesUtils.ToHex(type_hash.cmt_root_key_hash20_sha1) + " unkn2:" + BytesUtils.ToHex(type_hash.unkn2) + " Descr:" + Utils.CleanDescr(type_hash.description) + Environment.NewLine, EventType.Debug);
                    // Non e' calcolato sui dati presenti in questo file...
                    // A quanto pare il CMT_ROOT_KEY_HASH e' associato al cellulare.
                    // Log taken from JAF: CMT_ROOT_KEY_HASH:	 CAEEBB65D3C48E6DC73B49DC5063A2EE

                    // Sembra essere una grande perdita di tempo analizzare questi pacchetti perche' il loro interno non segue un formato determinabile... sembra piuttosto soggetto ad un protocollo di comunicazione.
                    //Content_Sha1 sha1 = new Content_Sha1(block.content);
                    //Debug.WriteLine(sha1);/**/
                    /*                                kkk++;
                                                    File.WriteAllBytes("c:\\file"+kkk+".bin", block.content);
                                                    /**/
                }
            }

#if DEBUG
            Debug.WriteLine(blocksColl);
#endif
            
            // Build rootKeys collection
            foreach (TBlock aBlock in blocksColl.GetPlainList())            
            {
                TBlockHeader27_ROFS_Hash block27 = aBlock.blockHeader as TBlockHeader27_ROFS_Hash;
                if (block27 == null)
                    continue;
                string sha1InHeader = BytesUtils.ToHex(block27.cmt_root_key_hash20_sha1).Replace(" ", "");
                List<TBlock> list = null;
                if (rootKeys.ContainsKey(sha1InHeader))
                    list = rootKeys[sha1InHeader];
                if (list == null)
                {
                    list = new List<TBlock>();
                    rootKeys.Add(sha1InHeader, list);
                }
                list.Add(aBlock);
            }
            
            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
        }


        public void DumpToFile()
        {
            FileStream fs_out = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs_out = new BufferedStream(fs_out);
            BinaryWriter bw_out = new BinaryWriter(bs_out);
            header.Write(bw_out);
            List<TGroup> groups = blocksColl.GetGroups();
            foreach (TGroup aGroup in groups)
            {
                aGroup.Write(bw_out);
            }
            bw_out.Close();
            bs_out.Close();
            fs_out.Close();

            // Fix Checksum
            FixCRCForWholeFile();
        }


        public Int16 CountBlocks(TASICType procType)
        {
            Int16 i = 0;
            foreach (TBlock aBlock in blocksColl.GetPlainList())
            {
                if (aBlock.blockHeader.flashMemory == procType)
                    i++;
            }
            return i;
        }


        private TFwType DoDetectType()
        {
            // Se c'e' il blocco 28 allora e' un FW di CORE...
            List<TBlock> blocks = blocksColl.GetPlainList();
            if (blocks.Count == 0)
                return TFwType.UNKNOWN;

            foreach (TBlock block in blocks)
            {
                if (block.blockType == TBlockType.BlockType28_CORE_Cert)
                    return TFwType.CORE;
            }
            // TODO: prende i primi 1000 per sicurezza, poi cerca il primo byte != 0...
            // TODO: usa i primi 100 bytes di dati per verificare il tipo di Firmware...
            // Se inizia con ROFS \ ROFX allora e' ROFS...
            // Se e' una immagine FAT16 allora e' UDA...

            List<byte> data = new List<byte>();
            int i = 0;
            while (i < blocks.Count && data.Count < 0x1000)
            {
                if (blocks[i].contentType == TContentType.Code)
                {
                    data.AddRange(blocks[i].content);
                }
                i++;
            }
            i = 0;
            while (i < data.Count && data[i] == 0) i++;
            //TODO: Che macello... potrebbero non essere tutti zeri... 
            //TODO: es: RM-217_0575441     rm217__07.30_ISM_005.image_b_era_pl_silver_v4
            //TODO: Salva l'offset in cui e' stata trovata l'immagine... tornera' utile per il repack...
            if (data.Count - i < 30)
                return TFwType.UNKNOWN;

            if (data[i] == 'R' && data[i + 1] == 'O' && data[i + 2] == 'F')
            {
                if (data[i + 3] == 'S')
                    return TFwType.ROFS;
                else
                    return TFwType.ROFX;
            }

            //File.WriteAllBytes("k:\\aaa.bin", data.ToArray());
            if (data.Count - i < 500)
                return TFwType.UNKNOWN;

            // bytesPerSector numFat    rootEntries 
            //Dec: 11-12      16        17-18
            if ((data[i + 0x0B] == 00 && data[i + 0x0C] == 02) &&
                (data[i + 0x10] >= 1 && data[i + 0x10] <= 2) &&
                (data[i + 0x11] == 00 && data[i + 0x12] <= 02))
                return TFwType.UDA;

            return TFwType.UNKNOWN;
        }


        public string FwTypeDescr
        {
            get
            {
                switch (FwType)
                {
                    case TFwType.ROFS:
                        return "ROFS";
                    case TFwType.ROFX:
                        return "ROFX";
                    case TFwType.CORE:
                        return "CORE";
                    case TFwType.UDA:
                        return "UDA";
                    default:
                        return "UNKNOWN";
                }
            }
        }

        public TFwType FwType
        {
            get
            {
                if (_fwType < 0)
                    _fwType = (int)DoDetectType();
                return (TFwType)_fwType;
            }
        }


        public Int64 ContentLength
        {
            get
            {
                TGroup aGroup = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
                if (aGroup == null)
                    return 0;
                Int64 length = 0;
                foreach (TBlock block in aGroup.codeBlocks)
                {
                    if (block.contentType == TContentType.Code)
                        length += block.content.Length;
                    //length += block.blockHeader.contentLength;                    
                }
                return length;
            }
        }


        private UInt32 LastValidLocation
        {
            get
            {
                TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
                UInt32 lowLimit, upLimit;
                header.GetEraseLimitsForAddr(group.FirstLocation, out lowLimit, out upLimit);

                if (lowLimit == 0 && upLimit == 0)
                {
                    if (FwType == TFwType.UDA)
                        upLimit = 0xFFFFFFFF;
                }

                if (upLimit == 0)
                {
                    if (FwType != TFwType.UNKNOWN)
                        throw new FwException("Can't detect Partition Data, Please contact m.bellino@symbian-toys.com");
                    // es: RM559_005_001_M021_16G
                    upLimit = group.LastLocation;
                    _canRemoveBlocks = false;
                    return upLimit;
                }

                if (FwType == TFwType.CORE)
                {
                    // For the CORE, the erase size includes ROFS1 + ROFS2 + ROFS3.
                    // So we use the informations contained in the section...
                    // return (group.LastLocation - group.FirstLocation);
                    TSectionArea sectArea = GetSectionArea();
                    List<TSectionInfo> sections = sectArea.GetRofsSections();

                    if (sections.Count < 1)
                    {
                        // es: N96
                        _canRemoveBlocks = false;
                        return group.LastLocation;
                    }
                    if (sections.Count < 2)
                        throw new FwException("Can't detect ROFS2 section, Please contact m.bellino@symbian-toys.com");
                    if (sections.Count > 3)
                        throw new FwException("Too many ROFS detected, Please contact m.bellino@symbian-toys.com");

                    TSectionInfo rofs1 = sections[0];
                    TSectionInfo rofs2 = sections[1];

                    // Non tiene conto dei byte di padding che ci sono al fondo dell'ultimo blocco.
                    upLimit = group.FirstLocation + rofs1.length;

                    // Serve per tenere conto anche del padding al fondo dell'ultimo blocco.
                    if (group.LastLocation > upLimit)
                    {
                        if (group.LastLocation - upLimit > 0x1000)
                            throw new FwException("LastValidLocation Padding > 0x1000, please contact m.bellino@symbian-toys.com");
                        // Si accerta pero' che non vada a sforare sino alla Rofs2
                        UInt32 maxRofs1Len = rofs2.startAddress - rofs1.startAddress;
                        if (group.LastLocation < group.FirstLocation + maxRofs1Len)
                            upLimit = group.LastLocation;
                    }
                }
                return upLimit;
            }
        }


        public UInt32 PartitionSize
        {
            get
            {
                TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
                UInt32 size = LastValidLocation - group.FirstLocation;
                return size;
            }
        }


        #region Repack Stuff


        private byte UpdateChecksum8(byte oldChecksum, byte oldByte, byte newByte)
        {
            byte twoCompl = (byte)(0xFF - oldByte + 1);
            byte newCheck8 = (byte)(0xFF - oldChecksum);
            newCheck8 += twoCompl;
            newCheck8 += newByte;
            newCheck8 = (byte)(0xFF - newCheck8);
            return newCheck8;
        }

        public void Repack(string newContentFile)
        {
            UInt32 lastOffset = 0;
            foreach (TBlock aBlock in blocksColl.GetPlainList())
            {
                // Non devo controllare la condizione di uguaglianza, perche' i blocchi appena aggiunti hanno tutti offset uguali.
                if (lastOffset > aBlock._offset)
                    throw new FwException("Error repacking fw, Block position has changed! Please contact: m.bellino@symbian-toys.com");
                lastOffset = aBlock._offset;
            }

            DateTime started = DateTime.Now;
            LogEvent("Insert new IMAGE: " + newContentFile + ". Please wait...\t");

            // Fa in modo che info sia una variabile locale...
            FileInfo info = new FileInfo(newContentFile);
            if (!info.Exists)
                throw new FwException("File: " + info.Name + " Doesn't Exists.");
            Int64 totDataBytesToWrite = _bytesToTrash + info.Length;

            /*            if (FwType == TFwType.ROFS && info.Length > ContentLength)
                            throw new FwLenException("New content exceeds the maximum length for ROFS image: " + (ContentLength) + " bytes.");
                        if (FwType == TFwType.ROFX && info.Length > ContentLength)
                            throw new FwLenException("New content exceeds the maximum length for ROFX image: " + (ContentLength) + " bytes.");/**/

            TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
            List<TBlock> blocks = group.codeBlocks;

            // ContentLength e' gia' comprensivo anche di _bytesToTrash
            Int64 currentSpaceForData = ContentLength;
            if (blocks.Count == 0)
                throw new FwException("No blocks present in the Firmware!");
            TBlock lastBlock = blocks[blocks.Count - 1];

            // The new length must be kept under the Partition Size.
            // Tiene conto anche di _bytesToTrash (dentro totDataBytesToWrite)
            Int64 FreeSpaceAfter = PartitionSize - totDataBytesToWrite;

            // Bisogna tenere conto anche dei blocchi descr, sottraiamo lo spazio occupato dal blocco descr. ( circa 0x400 )
            FreeSpaceAfter -= (group.LastLocationForCert - group.FirstLocationForCert);
            //    Non va bene la maniera seguente perche' perche' a quanto pare i Cert vengono scritti nella stessa locazione.
            //      foreach (TBlock aBlock in group.certBlocks)
            //          FreeSpaceAfter -= aBlock.blockHeader.contentLength;

            if (FreeSpaceAfter < 0)
                throw new FwException("Preliminary Check Failed: can't repack fw because the repacked size exeeds the Partition size, please remove " + BytesUtils.GetSizeDescr(-FreeSpaceAfter));
            //throw new FwException("Exceeds Partition Length: " + PartitionSize);
            /*
                        // Conosco il limite max di ROFS2 e ROFS3, quindi qui posso controllare se la location sfora...
                        // TODO: Per ROFS1, lo consento soltanto se proviene dal partition extender, che passera' un apposito flag.
                        // TODO: In tal caso, si suppone che l'immagine avra' gia' una dimensione corretta tale da occupare tutti i blocchi disponibili per ROFS1
                        if (FwType == TFwType.CORE)
                            throw new FwException("Can't repack fw because the ROFS1 size exeeds Partition size, please remove some data!");
            */

            // Removes Exceeding Blocks if the new file is smaller than current data...
            if (totDataBytesToWrite < currentSpaceForData && _canRemoveBlocks)
            {
                Int32 bytesToRemove = (Int32)(currentSpaceForData - totDataBytesToWrite);
                if (bytesToRemove > 0)
                {
                    while (bytesToRemove >= lastBlock.content.Length)
                    {
                        // Removes the last block
                        bytesToRemove -= lastBlock.content.Length;
                        blocks.Remove(lastBlock);
                        lastBlock = blocks[blocks.Count - 1];
                    }
                    /*                    // Trunk the last block
                                        lastBlock.blockHeader.contentLength -= (UInt32)bytesToRemove;
                                        Array.Resize(ref lastBlock.content, (Int32)lastBlock.blockHeader.contentLength);
                                        currentLength = ContentLength;*/
                }
                bytesToRemove = 0;
            }

            // Adds New Blocks if the new file is bigger than the current data...
            if (totDataBytesToWrite > currentSpaceForData)
            {
                Int32 bytesToAdd = (Int32)(totDataBytesToWrite - currentSpaceForData);
                Int32 fullBlockLength = 0x40000;
                if (blocks.Count > 1)
                    fullBlockLength = blocks[blocks.Count - 2].content.Length;

                Int32 leftSpaceInLastBlock = (Int32)(fullBlockLength - lastBlock.blockHeader.contentLength);

                // Insert data in the last block...
                if (bytesToAdd <= leftSpaceInLastBlock)
                {
                    // Fix Size for the last block
                    // Round size to 0x200
                    lastBlock.blockHeader.contentLength += (UInt32)bytesToAdd;
                    lastBlock.blockHeader.contentLength = BytesUtils.Round(lastBlock.blockHeader.contentLength, 0x200);
                    Array.Resize(ref lastBlock.content, (Int32)lastBlock.blockHeader.contentLength);
                    bytesToAdd = 0;
                }
                else
                {
                    lastBlock.blockHeader.contentLength += (UInt32)leftSpaceInLastBlock;
                    Debug.Assert(lastBlock.blockHeader.contentLength == fullBlockLength);
                    Array.Resize(ref lastBlock.content, (Int32)lastBlock.blockHeader.contentLength);
                    bytesToAdd -= leftSpaceInLastBlock;
                }

                // Do we need to add new Blocks?
                while (bytesToAdd > 0)
                {
                    TBlock newBlock = (TBlock)lastBlock.Clone();
                    newBlock.blockHeader.location = (UInt32)(lastBlock.blockHeader.location + lastBlock.content.Length);
                    //                        for (int i = 0; i < newBlock.content.Length; i++)
                    //                            newBlock.content[i] = 0xFF;

                    blocks.Add(newBlock);
                    lastBlock = blocks[blocks.Count - 1];

                    if (bytesToAdd <= lastBlock.blockHeader.contentLength)
                    {
                        // Fix Size for the last block
                        // Round size to 0x200
                        lastBlock.blockHeader.contentLength = (UInt32)bytesToAdd;
                        lastBlock.blockHeader.contentLength = BytesUtils.Round(lastBlock.blockHeader.contentLength, 0x200);
                        Array.Resize(ref lastBlock.content, (Int32)lastBlock.blockHeader.contentLength);
                        bytesToAdd = 0;
                    }
                    LogEvent("Add new Block:" + BytesUtils.ToHex(lastBlock.blockHeader.location) + " Size:" + BytesUtils.ToHex(lastBlock.blockHeader.contentLength) + "\n", EventType.Debug);
                    bytesToAdd -= (Int32)lastBlock.blockHeader.contentLength;
                }
            }

            // Questo check tiene conto anche dei blocchi Descr
            if (group.LastLocation > LastValidLocation)
                throw new FwException("Secondary Check Failed: Can't repack fw because the data exceeds the Partition size, please remove: " + BytesUtils.GetSizeDescr(group.LastLocation - LastValidLocation));

            // Wipe out existing data...
            Int32 totFreeSpace = 0;
            foreach (TBlock block in blocks)
            {
                if (block.contentType == TContentType.Code)
                {
                    Array.Clear(block.content, 0, block.content.Length);
                    //                    for (int i = 0; i < block.content.Length; i++)
                    //                        block.content[i] = 0xFF;
                    totFreeSpace += block.content.Length;
                }
            }
            totFreeSpace -= _bytesToTrash;

            // Replace Block Contents with the new Contents...
            bool addPadding = true;
            Int32 paddingBytes = _bytesToTrash;
            BufferedStream bs_in = new BufferedStream(info.OpenRead());

            if (totFreeSpace < bs_in.Length)
                throw new FwException("Not Enough Free Space, Please Contact: m.bellino@symbian-toys.com");

            foreach (TBlock block in blocks)
            {
                if (block.contentType == TContentType.Code)
                {
                    byte[] buffer = new byte[block.content.Length];
                    for (int i = 0; i < buffer.Length; i++)
                        buffer[i] = 0xFF;
                    //                    for (int i = 0; i < block.content.Length; i++)
                    //                        block.content[i] = 0xFF;
                    if (addPadding)
                    {
                        if (paddingBytes <= block.content.Length)
                        {
                            bs_in.Read(buffer, 0, (int)(block.content.Length - paddingBytes));
                            Debug.WriteLine(block.content.Length - paddingBytes);
                            Array.Copy(buffer, 0, block.content, paddingBytes, block.content.Length - paddingBytes);
                            addPadding = false;
                        }
                        paddingBytes -= block.content.Length;
                    }
                    else
                    {
                        bs_in.Read(buffer, 0, block.content.Length);
                        Array.Copy(buffer, block.content, buffer.Length);
                        Debug.WriteLine(block.content.Length);
                    }
                }
            }
            if (bs_in.Position < bs_in.Length)
            {
                bs_in.Close();
                throw new FwException("Repack Error, there exists still some data to write, Please contact m.bellino@symbian-toys.com");
            }
            bs_in.Close();

            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
            LogEvent("Firmware Repacked Successully!\n");

            DumpToFile();
        }
        #endregion


        private UInt32 GetCrcFromVpl()
        {
            string vplPath = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
            string[] files = Directory.GetFiles(vplPath, "*.vpl");
            if (files.Length <= 0)
            {
                throw new CRCException("*.vpl file not found in " + vplPath);
            }
            int i = 0;
            UInt32 crc = 0;
            bool gotCrc = false;
            while (i < files.Length && !gotCrc)
            {
                gotCrc = VplFile.GetCrcForFile(files[i], filename, out crc);
                i++;
            }
            if (!gotCrc)
            {
                string vplFiles = "";
                foreach (string s in files)
                {
                    if (vplFiles.Length > 0)
                        vplFiles += Environment.NewLine;
                    vplFiles += s;
                }
                string myFile = Path.GetFileName(filename.ToLower().Trim());
                throw new CRCException("CRC for file: " + myFile + " Not found in: " + Environment.NewLine + vplFiles);
            }
            return crc;
        }


        private void FixCRCForWholeFile()
        {
            if (header.offsetFixCrc == 0) // D4770E11
            {
                LogEvent("Can't fix CRC for this firmware, please contact: m.bellino@symbian-toys.com\n");
                return;
            }
            try
            {
                UInt32 vplCrc = GetCrcFromVpl();
                FixCRCForWholeFile(vplCrc, header.offsetFixCrc);
                LogEvent("CRC fixed using VPL file: 0x" + BytesUtils.ToHex(vplCrc) + "\n");
            }
            catch (CRCException ex)
            {
                FixCRCForWholeFile(_initCrc, header.offsetFixCrc);
                LogEvent(ex.Message + Environment.NewLine + "CRC fixed using original value: 0x" + BytesUtils.ToHex(_initCrc) + "\n");
            }
        }

        private void FixCRCForWholeFile(UInt32 newCRC, int offs)
        {
            DateTime started = DateTime.Now;
            LogEvent("Fix CRC value for firmware file. Please wait...\t");

            /*            byte[] allBytes = File.ReadAllBytes(filename);
                        Crc32_Fixer crc32_a = new Crc32_Fixer();
                        byte[] fixBytes_b = crc32_a.FixChecksum(allBytes, offs, newCRC);/**/

            byte[] fixBytes2 = null;
            // Fix CRC Using stream
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                Crc32_Fixer crc32 = new Crc32_Fixer();
                fixBytes2 = crc32.FixChecksum(br, offs, newCRC);
                br.Close();
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)))
            {
                bw.BaseStream.Seek(offs, SeekOrigin.Current);
                bw.Write(fixBytes2);
                bw.Close();
            }
            /*            byte[] allBytes = File.ReadAllBytes(filename);
                        Crc32_Fixer crc32 = new Crc32_Fixer();
                        byte[] fixBytes = crc32.FixChecksum(allBytes, offs, newCRC);
                        allBytes[offs + 0] = fixBytes[0];
                        allBytes[offs + 1] = fixBytes[1];
                        allBytes[offs + 2] = fixBytes[2];
                        allBytes[offs + 3] = fixBytes[3];
                        File.WriteAllBytes(filename, allBytes); */
            _initCrc = newCRC;

            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
        }


        #region Repartition
        public void RepartitionCore(UInt32 newRofs1Len, UInt32 newRofs2Len, UInt32 newRofs3Len, UInt32 newUdaLen)
        {
            if (FwType != TFwType.CORE)
                throw new FwException("Not CORE, Can't Repartition!");

            TSectionArea sectArea = GetSectionArea();
            TSectionInfo userSection = sectArea.GetUserSection();
            if (userSection == null)
                throw new FwException("Can't detect User Section! Please contact m.bellino@symbian-toys.com");
            List<TSectionInfo> rofsSections = sectArea.GetRofsSections();
            if (rofsSections.Count < 3)
                throw new FwException("This fw can't be repartitioned yet!");
            TSectionInfo rofs1 = rofsSections[0];

            TSectionInfo rofs2 = new TSectionInfo();
            rofs2.startAddress = rofs1.EndAddress;
            if (rofsSections.Count > 1)
                rofs2 = rofsSections[1];

            TSectionInfo rofs3 = new TSectionInfo();
            rofs3.startAddress = rofs2.EndAddress;
            if (rofsSections.Count > 2)
                rofs3 = rofsSections[2];

            if ((rofs1.length + rofs2.length + rofs3.length + userSection.length) !=
                (newRofs1Len + newRofs2Len + newRofs3Len + newUdaLen))
                throw new FwException("Wrong partition sizes! Please contact m.bellino@symbian-toys.com");

            if ((rofs1.EndAddress != rofs2.startAddress) ||
                (rofs2.EndAddress != rofs3.startAddress))
                throw new FwException("Gaps between ROFS not supported yet! Please contact m.bellino@symbian-toys.com");

            // Non devo rilocare i blocchi, l'inizio della ROFS1 non cambia            		

            // Rileva old UDA Len
            UInt32 oldUdaLen = userSection.length;
            UInt32 oldUdaEnd = userSection.EndAddress;
            Int32 udaSpaceNeeded = (Int32)(newUdaLen - oldUdaLen);

            // Fix Partition BB5 and Erase Area
            if (udaSpaceNeeded != 0)
            {
                TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
                TLV_Partition_Info_BB5 part = header.GetPartitionArea();
                part.ChangeSystemEnd(newRofs1Len + newRofs2Len + newRofs3Len);
                // Fix Erase Area
                header.ChangeEraseItemForAddr(group.FirstLocation, 0, -udaSpaceNeeded);
            }

            // Resize delle section.            
            UInt32 udaGap = userSection.startAddress - rofs3.EndAddress;
            rofs1.length = newRofs1Len;
            rofs2.startAddress = rofs1.EndAddress;
            rofs2.length = newRofs2Len;
            rofs3.startAddress = rofs2.EndAddress;
            rofs3.length = newRofs3Len;
            userSection.startAddress = rofs3.EndAddress + udaGap;
            userSection.length = newUdaLen;
            if (userSection.EndAddress != oldUdaEnd)
                throw new FwException("Wrong UDA End Address! Please contact m.bellino@symbian-toys.com");

            TBlock block = blocksColl.FindSectionAreaBlock();
            sectArea.WriteToSectionAreaBlock(block);
        }

        public void Repartition(Int32 lowOffset, Int32 upOffset)
        {
            if ((lowOffset == 0) && (upOffset == 0))
                return;

            DateTime started = DateTime.Now;
            LogEvent("Repartition " + FwTypeDescr + ". Please Wait...\t");
            if (FwType == TFwType.UNKNOWN)
                throw new FwException("Firmware Unknonwn, Can't Repartition!");

            TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
            UInt32 oldLowLimit, oldUpLimit;
            header.GetEraseLimitsForAddr(group.FirstLocation, out oldLowLimit, out oldUpLimit);

            // Fix Erase Area
            header.ChangeEraseItemForAddr(group.FirstLocation, lowOffset, upOffset);

            // Relocate Blocks
            if (lowOffset != 0)
            {
                foreach (TBlock aBlock in group.certBlocks)
                {
                    Int64 updatedLocation = aBlock.blockHeader.location + lowOffset;
                    if (aBlock.blockHeader.location >= oldLowLimit && aBlock.blockHeader.location <= oldUpLimit)
                    {
                        aBlock.blockHeader.location = (UInt32)updatedLocation;
                    }
                }
                foreach (TBlock aBlock in group.codeBlocks)
                {
                    Int64 updatedLocation = aBlock.blockHeader.location + lowOffset;
                    if (aBlock.blockHeader.location >= oldLowLimit && aBlock.blockHeader.location <= oldUpLimit)
                    {
                        aBlock.blockHeader.location = (UInt32)updatedLocation;
                    }
                }
            }
            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
        }
        #endregion


        public UInt32 ExtendRofs1()
        {
            TSectionArea sectArea = GetSectionArea();
            UInt32 res = sectArea.ExtendRofs1();
            TBlock block = blocksColl.FindSectionAreaBlock();
            sectArea.WriteToSectionAreaBlock(block);
            return res;
        }


        public TSectionArea GetSectionArea()
        {
            TBlock sectionBlock = blocksColl.FindSectionAreaBlock();
            TSectionArea area = new TSectionArea(sectionBlock);
            return area;
        }


        #region Extraction Methods

        public void ExtractRawImageToFile(string filename)
        {
            TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
            List<TBlock> blocksToUnpack = group.codeBlocks;
            if (blocksToUnpack == null)
                throw new FwException("BlockToUnpack = NULL, Please contact m.bellino@symbian-toys.com");
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock block in blocksToUnpack)
            {
                //                if (block.contentType != TContentType.Code)
                //                    continue;
                bs.Write(block.content, 0, block.content.Length);
            }
            bs.Close();
            fs.Close();

#if DEBUG
            
/*            if (FwType == TFwType.UDA || FwType == TFwType.UNKNOWN)
                return;
            Content_Sha1 blk = new Content_Sha1(group.descrBlock.content);
            ExtractGroupsToPath(@"C:\Users\Root\Desktop\Extr\");

            //byte[] allBytes2 = group.list[0].content; 
            byte[] allBytes2 = File.ReadAllBytes(filename);
            string sha1_in_block = BytesUtils.ToHex(blk.hash20_sha1);
            sha1_in_block = sha1_in_block.Replace(" ", "");

            long dataLen1 = blk.length2 - group.descrBlock.content.Length;
            byte[] newArr1 = new byte[dataLen1];
            Array.Copy(allBytes2, 0, newArr1, 0, newArr1.Length);
            string sha1_1 = Analysis.ComputeSHA1(newArr1).Replace(" ", "");
            if (sha1_in_block != sha1_1)
            {
            }/**/
#endif
        }

        public void ExtractAlignedCodeToFile(string filename)
        {
            TGroup group = blocksColl.GetRofsOrUdaGroupForFwType(FwType);
            if (group == null)
                throw new FwException("Firmware File Format Unknown, Please contact m.bellino@symbian-toys.com");
            List<TBlock> blocksToUnpack = group.codeBlocks;

            if (blocksToUnpack == null)
                throw new FwException("BlockToUnpack = NULL, Please contact m.bellino@symbian-toys.com");

            _bytesToTrash = 0;
            DateTime started = DateTime.Now;
            LogEvent("Extract IMAGE from fw. Please wait...\t");
            bool skipBytes = true;
            //skipBytes = false;      // TODO: verifica se mantenere o rimuovere il padding iniziale
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock block in blocksToUnpack)
            {
                if (block.contentType != TContentType.Code)
                    continue;
                if (!skipBytes)
                {
                    bs.Write(block.content, 0, block.content.Length);
                    continue;
                }
                // skipbytes and Block.Code
                int i = 0;
                while (i < block.content.Length && block.content[i] == 0)
                    i++;
                _bytesToTrash += i;

                if (_bytesToTrash > 0)
                {
                    Debug.WriteLine(BytesUtils.ToHex(block.blockHeader.location) + " + " + BytesUtils.ToHex((UInt16)_bytesToTrash) + " = " + BytesUtils.ToHex((UInt32)(block.blockHeader.location + _bytesToTrash)));
                }
                if (i < block.content.Length)
                {
                    byte[] alignedContent = new byte[block.content.Length - i];
                    Array.Copy(block.content, i, alignedContent, 0, alignedContent.Length);
                    bs.Write(alignedContent, 0, alignedContent.Length);
                    skipBytes = false;
                }
            }
            bs.Close();
            fs.Close();
            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
            LogEvent("IMAGE extracted to file: " + filename + "\n");
        }


        public void ExtractMemoryTypeToFile(string filename, TASICType type)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock block in blocksColl.GetPlainList())
            {
                if (block.blockHeader.flashMemory == type)
                    bs.Write(block.content, 0, block.content.Length);
            }
            bs.Close();
            fs.Close();
        }


        public void ExtractBlockTypeToFile(string filename, TBlockType type)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock block in blocksColl.GetPlainList())
            {
                if (block.blockType == type)
                    bs.Write(block.content, 0, block.content.Length);
            }
            bs.Close();
            fs.Close();
        }


        public void ExtractContentTypeToFile(string filename, TContentType type)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock block in blocksColl.GetPlainList())
            {
                if (block.contentType == type)
                    bs.Write(block.content, 0, block.content.Length);
            }
            bs.Close();
            fs.Close();
        }


        public void ExtractGroupsToPath(string aPath)
        {
            Directory.CreateDirectory(aPath);
            foreach (TLV aTlv in header.tlvData)
            {
                /*if (aTlv.type == TTLVType.AlgoSendingSpeed ||
                    aTlv.type == TTLVType.Array ||
                    aTlv.type == TTLVType.APE_Algorithm ||
                    aTlv.type == TTLVType.APE_Phone_Type ||
                    aTlv.type == TTLVType.APE_SupportedHW ||
                    aTlv.type == TTLVType.CMT_Algo ||
                    aTlv.type == TTLVType.CMT_SupportedHW ||
                    aTlv.type == TTLVType.CMT_Type ||
                    aTlv.type == TTLVType.DateTime ||
                    aTlv.type == TTLVType.Descr ||
                    aTlv.type == TTLVType.ERASE_AREA_BB5 ||
                    aTlv.type == TTLVType.ERASE_DCT5 ||
                    aTlv.type == TTLVType.FORMAT_PARTITION_BB5 ||
                    aTlv.type == TTLVType.MessageReadingSpeed ||
                    aTlv.type == TTLVType.ProgramSendingSpeed ||
                    aTlv.type == TTLVType.PARTITION_INFO_BB5 ||
                    aTlv.type == TTLVType.SecondarySendingSpeed)
                continue;*/
                string s = aTlv.type.ToString();
                string filename = aPath + aTlv._offset + "_" + BytesUtils.ToHex(aTlv._offset) + "_" + s + ".bin";
                File.WriteAllBytes(filename, aTlv.value);
            }

            foreach (TGroup aGroup in blocksColl.GetGroups())
            {
                /*                Debug.WriteLine(aGroup);
                                UInt32 low, up = 0;
                                header.GetEraseLimits(aGroup.FirstLocation, out low, out up);
                                Debug.WriteLine("Low: " + BytesUtils.ToHex(low) + " Hi: "+BytesUtils.ToHex(up));/**/

                string blockDescr = "";
                foreach (TBlock aBlock in aGroup.certBlocks)
                {
                    TBlockHeader27_ROFS_Hash dataBlock = aBlock.blockHeader as TBlockHeader27_ROFS_Hash;
                    blockDescr = dataBlock.description;
                    blockDescr = blockDescr.Replace("\0", "");
                    blockDescr = blockDescr.Replace("*", "-");
                    string filename = aPath + aBlock._offset + "_" + BytesUtils.ToHex(aBlock._offset) + "_" + blockDescr + "_" + BytesUtils.ToHex((byte)aBlock.blockType) + ".bin";
                    File.WriteAllBytes(filename, aBlock.content);
                }

                if (aGroup.codeBlocks.Count > 0)
                {
                    TBlock firstBlock = aGroup.codeBlocks[0];
                    string filename = aPath + firstBlock._offset + "_" + BytesUtils.ToHex(firstBlock._offset) + "_" + blockDescr + "_" + BytesUtils.ToHex((byte)firstBlock.blockType) + ".bin";
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    BufferedStream bs = new BufferedStream(fs);
                    foreach (TBlock aBlock in aGroup.codeBlocks)
                    {
                        fs.Write(aBlock.content, 0, aBlock.content.Length);
                    }
                    bs.Close();
                    fs.Close();
                }
            }
        }


        // Useful for investigation purposes
        public void ExtractAllContentsToPath(string path)
        {
            Directory.CreateDirectory(path);
            path += Path.DirectorySeparatorChar;
            ExtractMemoryTypeToFile(path + "Mem_APE.bin", TASICType.APE);
            ExtractMemoryTypeToFile(path + "Mem_CMT.bin", TASICType.CMT);
            ExtractBlockTypeToFile(path + "Blocks_17.bin", TBlockType.BlockType17);
            ExtractBlockTypeToFile(path + "Blocks_2E.bin", TBlockType.BlockType2E);
            ExtractBlockTypeToFile(path + "Blocks_30.bin", TBlockType.BlockType30);
            ExtractBlockTypeToFile(path + "Blocks_27_ROFS.bin", TBlockType.BlockType27_ROFS_Hash);
            ExtractBlockTypeToFile(path + "Blocks_28_CORE.bin", TBlockType.BlockType28_CORE_Cert);
            ExtractContentTypeToFile(path + "Content_Code.bin", TContentType.Code);
            ExtractContentTypeToFile(path + "Content_Data.bin", TContentType.DataCert);
        }
        #endregion

    }
}


