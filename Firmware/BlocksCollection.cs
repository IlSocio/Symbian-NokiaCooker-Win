using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace Firmware
{

    [Serializable]
    public class TBlock : ICloneable
    {
        public UInt32 _offset = 0;
        public TContentType contentType;
        public byte const01;
        public TBlockType blockType;
        public byte blockHeaderSize;
        public TBlockHeader blockHeader;
        public byte blockChecksum8;
        public byte[] content;

        public TBlock()
        {
        }

        public void Read(BinaryReader br)
        {
            contentType = (TContentType)br.ReadByte();
            const01 = br.ReadByte();
            if (const01 != 01)
                throw new FwException("Const != 01, Please Contact: m.bellino@symbian-toys.com");

            blockType = (TBlockType)br.ReadByte();
            blockHeaderSize = br.ReadByte();


            // Compute the Checksum as 0xFF - const01 - blockType - blockSize - blockHeader[] .... 
            // Credits for this Checksum goes to the PNHT
            byte[] rawData = br.ReadBytes(blockHeaderSize);
            br.BaseStream.Seek(-blockHeaderSize, SeekOrigin.Current);

            // Useful to check for errors
            long oldPos = br.BaseStream.Position;
            blockHeader = TBlockHeader.Factory(br, blockType);
            blockHeader.Read(br);
            if (br.BaseStream.Position != oldPos + blockHeaderSize)
            {
                long missing = (oldPos + blockHeaderSize - br.BaseStream.Position);
                if (missing != 0)
                    throw new FwException("Error Reading Header! " + missing + " bytes left. Please contact m.bellino@symbian-toys.com");
                // Fixes the offset and continue...
                if (missing > 0)
                {
                    byte[] trash = br.ReadBytes((int)missing);
                }
                else
                {
                    br.BaseStream.Seek(missing, SeekOrigin.Current);
                }
            }

            blockChecksum8 = br.ReadByte();
#if DEBUG
            byte computedCrc8 = Analysis.ComputeChecksum8(const01, blockType, blockHeaderSize, rawData);
            Debug.Assert(blockChecksum8 == computedCrc8, "Wrong Block Checksum8");
#endif

            content = br.ReadBytes((int)blockHeader.contentLength);
            if (content.Length != blockHeader.contentLength)
                throw new FwException("Firmware File is Corrupted. Length != ContentLength, you should download it again!");
            /*            File.WriteAllBytes("c:\\f1.bin", content);
                        Array.Resize(ref content, content.Length - 16 * 3);
                        byte[] arr = new byte[16*8];
                        Array.Copy(content, content.Length - 16*8, arr, 0, 16 * 8);
                        File.WriteAllBytes("c:\\f2.bin", arr);*/
#if DEBUG
            UInt16 computedCheck16 = Analysis.ComputeChecksum16(content);
            Debug.Assert(blockHeader.contentChecksum16 == computedCheck16, "Wrong Content Checksum16");
#endif
            // File.WriteAllBytes("c:\\f1.bin", content);
            UInt32 roudedLen = BytesUtils.Round(blockHeader.contentLength, 0x200);
            //Debug.Assert(roudedLen == content.Length, "Length is not aligned, Please Contact: m.bellino@symbian-toys.com");
        }


        public void UpdateChecksums()
        {
            // Updates the Checksum16...
            blockHeader.contentChecksum16 = Analysis.ComputeChecksum16(content);

            // Updates the Checksum8...
            byte[] blockHeaderBuffer = new byte[blockHeaderSize];
            BinaryWriter bw = new BinaryWriter(new MemoryStream(blockHeaderBuffer));
            blockHeader.Write(bw);
            bw.Close();
            blockChecksum8 = Analysis.ComputeChecksum8(const01, blockType, blockHeaderSize, blockHeaderBuffer);
        }


        public void Write(BinaryWriter bw)
        {
            UpdateChecksums();
            bw.Write((byte)contentType);
            bw.Write(const01);
            bw.Write((byte)blockType);
            bw.Write(blockHeaderSize);
            blockHeader.Write(bw);
            bw.Write(blockChecksum8);
            bw.Write(content);
        }

        public override string ToString()
        {
            return "ContentType_" + BytesUtils.ToHex((byte)contentType) + " TBlock_" + BytesUtils.ToHex((byte)blockType) + " " + blockHeader.ToString() + "   " + BytesUtils.ToHex(blockHeader.FirstLocation) + " - " + BytesUtils.ToHex(blockHeader.LastLocation);
        }

        #region ICloneable

        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

        #endregion
    }


    public class TGroup
    {
        public List<TBlock> certBlocks = new List<TBlock>();
        public List<TBlock> codeBlocks = new List<TBlock>();
        public List<TBlock> unknBlocks = new List<TBlock>();

        public TGroup()
        {
        }

        public List<TBlock> GetAll()
        {
            List<TBlock> res = new List<TBlock>();
            res.AddRange(certBlocks);
            res.AddRange(codeBlocks);
            return res;
        }

        public TGroup(TBlock aBlock)
        {
            AddBlock(aBlock);
        }

        public void AddBlock(TBlock aBlock)
        {
#if DEBUG
            switch (aBlock.blockType)
            {
                case TBlockType.BlockType27_ROFS_Hash:
                    certBlocks.Add(aBlock);
                    break;
                case TBlockType.BlockType28_CORE_Cert:
                    certBlocks.Add(aBlock);
                    break;
                default:
                    codeBlocks.Add(aBlock);
                    break;
            }
            return;
#endif
            switch (aBlock.contentType)
            {
                case TContentType.Unkn1:
                    unknBlocks.Add(aBlock);
                    break;
                case TContentType.DataCert:
                    certBlocks.Add(aBlock);
                    break;
                case TContentType.Code:
                    codeBlocks.Add(aBlock);
                    break;
                default:
                    throw new FwException("ContentType unknown");
            }
        }

        public UInt32 FirstLocationForCert
        {
            get
            {
                if (certBlocks.Count > 0)
                    return certBlocks[0].blockHeader.FirstLocation;
                return 0;
            }
        }

        public UInt32 LastLocationForCert
        {
            get
            {
                if (certBlocks.Count > 0)
                    return certBlocks[certBlocks.Count - 1].blockHeader.LastLocation;
                return 0;
            }
        }

        public UInt32 FirstLocation
        {
            get
            {
                if (certBlocks.Count > 0)
                    return certBlocks[0].blockHeader.FirstLocation;
                if (codeBlocks.Count > 0)
                    return codeBlocks[0].blockHeader.FirstLocation;
                return 0;                
            }
        }

        public UInt32 LastLocation
        {
            get
            {
                if (codeBlocks.Count > 0)
                    return codeBlocks[codeBlocks.Count - 1].blockHeader.LastLocation;
                if (certBlocks.Count > 0)
                    return certBlocks[certBlocks.Count - 1].blockHeader.LastLocation;
                return 0;
            }
        }


        public string Descr
        {
            get
            {
                string descr = "[NONE]";
                if (certBlocks.Count > 0)
                {
                    descr = "[UNKN]";
                    TBlock descrBlock = certBlocks[0];
                    TBlockHeader27_ROFS_Hash headBlock = descrBlock.blockHeader as TBlockHeader27_ROFS_Hash;
                    if (headBlock != null)
                    {
                        descr = "[" + headBlock.description + "]";
                        descr = descr.Replace("\0", "");
                        descr = descr.Replace("*", "-");
                    }
                }
                return descr;
            }
        }


        public override string ToString()
        {
            return Descr + " " + BytesUtils.ToHex(FirstLocation) + " - " + BytesUtils.ToHex(LastLocation) + " " + certBlocks.Count + "+" + codeBlocks.Count;
        }


        public void Write(BinaryWriter bw)
        {
            foreach (TBlock aBlock in certBlocks)
                aBlock.Write(bw);
            foreach (TBlock aBlock in codeBlocks)
                aBlock.Write(bw);
            foreach (TBlock aBlock in unknBlocks)
                aBlock.Write(bw);
        }
    }



    public class TGroupCollection
    {
        private TBlock _lastBlock = null;
        private List<TGroup> _groups = new List<TGroup>();

        public TBlock FindSectionAreaBlock()
        {
            int i = 0;
            while (_groups[i].certBlocks.Count == 0)
                i++;
            i--;
            if (i < 0)
                throw new FwException("Sections Block not Found, please contact m.bellino@symbian-toys.com");
            if (_groups[i].codeBlocks.Count != 1)
                throw new FwException("Sections Block != 1, please contact m.bellino@symbian-toys.com");
            TBlock sectionBlock = _groups[i].codeBlocks[0];
            return sectionBlock;
        }

        public TGroup GetGroupMatchingDescrition(string descr)
        {
            TGroup res = null;
            descr = descr.ToLower();
            foreach (TGroup aGroup in _groups)
            {
                if (aGroup.certBlocks.Count <= 0)
                    continue;
                TBlock descrBlock = aGroup.certBlocks[0];
                TBlockHeader27_ROFS_Hash blockHead = descrBlock.blockHeader as TBlockHeader27_ROFS_Hash;
                //Debug.WriteLine("DESCR:" + blockHead.description.Replace("\0", ""));
                if (blockHead.description.ToLower().Contains(descr))
                {
                    res = aGroup;
                }
            }
            return res;
        }

        public void AddBlock(TBlock aBlock)
        {
            if (_lastBlock == null)
            {
                // Insert the first block
                TGroup group = new TGroup(aBlock);
                _groups.Add(group);
                _lastBlock = aBlock;
                return;
            }

            // Se ha gia' aggiunto dei blocchi Code ed ora deve aggiungere dei blocchi Data, allora forza la creazione di un nuovo Gruppo.
            if (_lastBlock.contentType == TContentType.Code &&
                aBlock.contentType == TContentType.DataCert)
            {
                TGroup group = new TGroup(aBlock);
                _groups.Add(group);
                _lastBlock = aBlock;
                return;
            }

            // Se il blocco e' contiguo, oppure sovrascrive il vecchio allora lo aggiunge.
            if ((aBlock.blockHeader.FirstLocation <= _lastBlock.blockHeader.LastLocation) &&
               (aBlock.blockHeader.FirstLocation >= _lastBlock.blockHeader.FirstLocation))
            {
                TGroup lastGroup = _groups[_groups.Count - 1];
                lastGroup.AddBlock(aBlock);
                _lastBlock = aBlock;
                return;
            }

            TGroup newGroup = new TGroup(aBlock);
            _groups.Add(newGroup);
            _lastBlock = aBlock;

            /*            if (aBlock.blockHeader.FirstLocation != _lastBlock.blockHeader.LastLocation)
                        {
                            TGroup group = new TGroup(aBlock);
                            _groups.Add(group);
                        }
                        else
                        {
                            TGroup lastGroup = _groups[_groups.Count - 1];
                            lastGroup.AddBlock(aBlock);
                        }*/
        }

        public void Clear()
        {
            _lastBlock = null;
            _groups.Clear();
        }

        public List<TBlock> GetPlainList()
        {
            List<TBlock> res = new List<TBlock>();
            foreach (TGroup aGroup in _groups)
            {
                foreach (TBlock aBlock in aGroup.certBlocks)
                    res.Add(aBlock);
                foreach (TBlock aBlock in aGroup.codeBlocks)
                {
#if DEBUG
                    if (aBlock.blockHeader.FirstLocation >= 0x400000)
#endif
                        res.Add(aBlock);
                }
            }
            return res;
        }


        public TGroup GetRofsOrUdaGroupForFwType(TFwType fwType)
        {
            switch (fwType)
            {
                case TFwType.CORE:
                case TFwType.ROFS:
                case TFwType.ROFX:
                    {
                        TGroup res = GetGroupMatchingDescrition("ROFS");
                        if (res == null)
                            res = GetGroupMatchingDescrition("ROFX");
                        if (res == null)
                            res = GetGroupMatchingDescrition("MCUSW");
                        if (res == null)
                        {
                            // TODO: es RM84 50737301_RM84rd.v57
                            if (_groups.Count > 0)
                                return _groups[0];
                            throw new FwException("This Firmware is not supported yet, Please Contact m.bellino@symbian-toys.com");
                        }
                        return res;
                    }
                case TFwType.UDA:
                    {
                        if (_groups.Count > 1)
                            throw new FwException("UDA contains more than 1 group, Please Contact m.bellino@symbian-toys.com");
                        TGroup group = _groups[0];
                        if (group.certBlocks.Count > 0)
                            throw new FwException("UDA group contains description block, Please Contact m.bellino@symbian-toys.com");
                        if (group.codeBlocks.Count == 0)
                            throw new FwException("UDA group doesn't contains any block, Please Contact m.bellino@symbian-toys.com");
                        return group;
                    }
                default:
                    {
                        TGroup tmpGroup = new TGroup();
                        foreach (TBlock aBlock in GetPlainList())
                        {
                            tmpGroup.AddBlock(aBlock);
                        }
                        return tmpGroup;
                    }
            }
            return null;
        }


        public List<TGroup> GetGroups()
        {
            return _groups;
        }

        public override string ToString()
        {
            string res = "";
            foreach (TGroup aGroup in _groups)
            {
                res += aGroup.ToString() + Environment.NewLine;
            }
            return res;
        }
    }

}
