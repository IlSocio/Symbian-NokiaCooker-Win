
#define ADD_SIGNATURE

using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Diagnostics;
using FuzzyByte.Utils;


namespace Firmware
{
    public enum EFatType
    {
        FAT16,
        FAT32
    }

    public class BootRecord
    {
        public long _offset = 0;
        public byte[] jump = new byte[3];
        public byte[] oem_name = new byte[8];
        public UInt16 bytesPerSector;
        public byte sectorsPerCluster;
        public UInt16 reservedSector;   // The number of sectors preceding the start of the first FAT, including the boot sector.
        public byte numFats;
        public UInt16 rootEntries;      // 0 for FAT32
        public UInt16 totalSectors16;     // set if the partiton is less than 33,554,432 bytes (32mb) in size - mainly for a floppy disk: 40,0b flip -> 0b,40 convert to decimal = 2880 sectors; fat16 partitions may also use this entry. if number of sectors is above this will be set to 00,00
        public byte mediaType;
        public UInt16 sectorsPerFat16;    // On FAT32 volumes this field must be 0, and BPB_FATSz32 contains the FAT size count.
        public UInt16 sectorsPerTrack;
        public UInt16 numHeads;
        public UInt32 hiddenSectors;    // The number of sectors on the volume before the boot sector. This value is used during the boot sequence to calculate the absolute offset to the root directory and data areas. 
        public UInt32 totalSectors32;   // This field is the new 32-bit total count of sectors on the volume. This count includes the count of all sectors in all four regions of the volume. 
                                        // This field can be 0; if it is 0, then BPB_TotSec16 must be non-zero. 
                                        // For FAT32 volumes, this field must be non-zero. 
                                        // For FAT12/FAT16 volumes, this field contains the sector count if BPB_TotSec16 is 0 (count is greater than or equal to 0x10000).

        // (Offset 36) FAT12 and FAT16
        public byte driveId;            // Physical Drive Number
        public byte reserved;
        public byte extendedBootSign;   // set to 29 indicating that the serial, label and type data is present.
        public UInt32 volumeSerial;
        public byte[] volumeName = new byte[11];
        public byte[] filSysType16 = new byte[8];    // One of the strings “FAT12   ”, “FAT16   ”, or “FAT     ”.  NOTE: Many people think that the string in this field has something to do with the determination of what type of FAT—FAT12, FAT16, or FAT32—that the volume has. This is not true. You will note from its name that this field is not actually part of the BPB. This string is informational only and is not used by Microsoft file system drivers to determine FAT typ,e because it is frequently not set correctly or is not present. See the FAT Type Determination section of this document. This string should be set based on the FAT type though, because some non-Microsoft FAT file system drivers do look at it.
        public byte[] execBootCode = new byte[448];
        public UInt16 execSign;

        // (Offset 36) FAT32
        public UInt32 sectorsPerFat32;
        public UInt16 extFlags;
        public UInt16 fsVer;
        public UInt32 rootClust;
        public UInt16 fsInfo;
        public UInt16 bkBootRec;
        public byte[] reservedFAT32 = new byte[12];
        public byte drvNum32;
        public byte reservedFAT32_b;
        public byte bootSig;
        public UInt32 volId;
        public byte[] volLabel = new byte[11];
        public byte[] filSysType32 = new byte[8];
        
        private UInt32 _countOfClusters;
        private EFatType _fatType = EFatType.FAT16;


        public UInt32 CountOfClusters
        {
            get
            {
                return _countOfClusters;
            }
        }

        public EFatType FatType
        {
            get
            {
                return _fatType;
            }
            private set
            {
                _fatType = value;
            }
        }


        public void Read(BinaryReader br)
        {
            _offset = br.BaseStream.Position;
            jump = br.ReadBytes(jump.Length);
            oem_name = br.ReadBytes(oem_name.Length);
            bytesPerSector = br.ReadUInt16();
            sectorsPerCluster = br.ReadByte();
            reservedSector = br.ReadUInt16();
            numFats = br.ReadByte();
            rootEntries = br.ReadUInt16();
            totalSectors16 = br.ReadUInt16();
            mediaType = br.ReadByte();
            sectorsPerFat16 = br.ReadUInt16();
            sectorsPerTrack = br.ReadUInt16();
            numHeads = br.ReadUInt16();
            hiddenSectors = br.ReadUInt32();
            totalSectors32 = br.ReadUInt32();

            // READ FAT32
            sectorsPerFat32 = br.ReadUInt32();
            extFlags = br.ReadUInt16();
            fsVer = br.ReadUInt16();
            rootClust = br.ReadUInt32();
            fsInfo = br.ReadUInt16();
            bkBootRec = br.ReadUInt16();            // 6
            reservedFAT32 = br.ReadBytes(12);
            drvNum32 = br.ReadByte();
            reservedFAT32_b = br.ReadByte();
            bootSig = br.ReadByte();
            volId = br.ReadUInt32();
            volLabel = br.ReadBytes(11);
            filSysType32 = br.ReadBytes(8);

            // READ FAT16
            br.BaseStream.Seek(36, SeekOrigin.Begin);
            driveId = br.ReadByte();
            reserved = br.ReadByte();
            extendedBootSign = br.ReadByte();
            volumeSerial = br.ReadUInt32();
            volumeName = br.ReadBytes(volumeName.Length);
            filSysType16 = br.ReadBytes(filSysType16.Length);
            execBootCode = br.ReadBytes(execBootCode.Length);
            execSign = br.ReadUInt16();



            /*
            A FAT file system volume is composed of four basic regions, which are laid out in this order on the volume:
            0 – Reserved Region
            1 – FAT Region
            2 – Root Directory Region (doesn’t exist on FAT32 volumes)
            3 – File and Directory Data Region
            */

            // ************ I CALCOLI CHE SEGUONO ARRIVANO DALLA REFERENCE UFFICIALE
            // First, we determine the count of sectors occupied by the root directory as noted earlier.
            UInt32 rootDirSectors = (UInt32)(rootEntries * 32);
            rootDirSectors = rootDirSectors + bytesPerSector - 1;
            rootDirSectors /= bytesPerSector;

            // rootDirSectors = 0 on FAT32
            UInt32 fatSize = sectorsPerFat16;
            if (fatSize == 0)
                fatSize = sectorsPerFat32;

            UInt32 totalSectors = totalSectors16;
            if (totalSectors == 0)
                totalSectors = totalSectors32;

            UInt32 dataSec = totalSectors - (reservedSector + numFats * fatSize + rootDirSectors);
            _countOfClusters = dataSec / sectorsPerCluster;
            if (_countOfClusters < 4085)
            {
                throw new Fat16Exception("FAT 12 is NOT Supported yet! Please contact m.bellino@symbian-toys.com");
                /* Volume is FAT12 */
            }
            else if (_countOfClusters < 65525)
            {
                FatType = EFatType.FAT16;
                /* Volume is FAT16 */
                rootClust = 0;
            }
            else
            {
                FatType = EFatType.FAT32;
                /* Volume is FAT32 */
                if (rootClust != 2)
                    throw new Fat16Exception("Root Cluster is NOT Supported! Please contact: m.bellino@symbian-toys.com");
                if (fsInfo > 1)
                    throw new Fat16Exception("FSInfo is NOT Supported! Please contact: m.bellino@symbian-toys.com");
                //if (bkBootRec != 0)
                //    throw new Fat16Exception("Backup Boot Record is NOT Supported! Please contact: m.bellino@symbian-toys.com");
            }
            // ************ QUESTI CALCOLI ARRIVANO DALLA REFERENCE UFFICIALE        

#if ADD_SIGNATURE
            // NC35 Signature
            byte[] newVolume = new byte[] { (byte)'N', (byte)'C', (byte)'3', (byte)'5', 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            if (FatType == EFatType.FAT32)
                Array.Copy(newVolume, volLabel, volLabel.Length);
            if (FatType == EFatType.FAT16)
                Array.Copy(newVolume, volumeName, volLabel.Length);
#endif
        }


        public void Resize(UInt32 newSize) 
        {
//                throw new FwException("Sorry, resize of FAT16 UDA is not supported yet. Currently Only FAT32 is supported!");

            // FAT32 min. 65525 clusters * 1 sectPerCluster * bytesPerSector
            // FAT32 max. 268,435,444 clusters * 16 sectPerCluster * bytesPerSector
            // FAT16 min. 4085 clusters  * 1 sectPerCluster * bytesPerSector
            // FAT16 max. 65524 clusters * 64 sectPerCluster * bytesPerSector

            UInt32 totalSectors = newSize / bytesPerSector;
//            if (totalSectors < 0x10000 && FatType == EFatType.FAT32)
//                throw new FwException("Change FAT32 to FAT16 is not supported. This UDA FAT32 must be greater than " + BytesUtils.GetSizeDescr(0x10000 * 512));
                // deve essere >= 0x10000
//            if (totalSectors < 0x10000 && FatType == EFatType.FAT16)
//                throw new FwException("Change FAT16 to FAT12 is not supported. This UDA FAT16 must be greater than " + BytesUtils.GetSizeDescr(0x10000 * 512));

            UInt32 rootDirSectors = (UInt32)(rootEntries * 32);
            rootDirSectors = rootDirSectors + bytesPerSector - 1;
            rootDirSectors /= bytesPerSector;

            UInt32 tmpVal1 = totalSectors - (reservedSector + rootDirSectors);
            UInt32 tmpVal2 = (UInt32)((256 * sectorsPerCluster) + numFats);

            UInt32 sectorsPerFat = 0;
            if (FatType == EFatType.FAT32)
            {
                tmpVal2 = tmpVal2 / 2;
                sectorsPerFat16 = 0;
                sectorsPerFat32 = (tmpVal1 + (tmpVal2 - 1)) / tmpVal2;
                totalSectors16 = 0;
                totalSectors32 = totalSectors;
                sectorsPerFat = sectorsPerFat32;
            }
            else
            {
                sectorsPerFat16 = (UInt16)((tmpVal1 + (tmpVal2 - 1)) / tmpVal2);
                sectorsPerFat32 = 0;
                totalSectors16 = 0;
                totalSectors32 = 0;
                if (totalSectors < 0x10000)
                {
                    totalSectors16 = (UInt16)totalSectors;
                }
                else
                {
                    totalSectors32 = totalSectors;
                }
                sectorsPerFat = sectorsPerFat16;
            }

            // Updates the countOfClusters
            UInt32 dataSec = totalSectors - (reservedSector + numFats * sectorsPerFat + rootDirSectors);
            _countOfClusters = dataSec / sectorsPerCluster;


            if (FatType == EFatType.FAT32)
            {
                // FAT32 min. 65525 clusters * 1 sectPerCluster * bytesPerSector
                // FAT32 max. 268,435,444 clusters * 16 sectPerCluster * bytesPerSector
                if (_countOfClusters < 65525)
                {
                    UInt32 minSize = (UInt32)(65525 * sectorsPerCluster * bytesPerSector);
                    throw new FwException("Change FAT32 > FAT16, or change in Cluster Size, is not supported. This UDA FAT16 must be greater than " + BytesUtils.GetSizeDescr(minSize));
                }
            }
            else
            {
                // FAT16 min. 4085 clusters  * 1 sectPerCluster * bytesPerSector
                // FAT16 max. 65524 clusters * 64 sectPerCluster * bytesPerSector
                if (_countOfClusters < 4085)
                {
                    UInt32 minSize = (UInt32)(4085 * sectorsPerCluster * bytesPerSector);
                    throw new FwException("Change FAT16 > FAT12, or change in Cluster Size, is not supported. This UDA FAT16 must be greater than " + BytesUtils.GetSizeDescr(minSize));
                }
                if (_countOfClusters > 65524)
                {
                    UInt32 maxSize = (UInt32)(65524 * sectorsPerCluster * bytesPerSector);
                    throw new FwException("Change FAT16 > FAT32, or change in Cluster Size, is not supported. This UDA FAT16 must be lesser than " + BytesUtils.GetSizeDescr(maxSize));
                }
            }

            UInt32 bytesPerFat = sectorsPerFat * bytesPerSector;            
            if (FatType == EFatType.FAT32)
            {
                if (bytesPerFat < CountOfClusters * 4)
                    throw new FwException("Wrong bytesPerFat32 computation.");
            } else
            {
                if (bytesPerFat < CountOfClusters * 2)
                    throw new FwException("Wrong bytesPerFat16 computation.");
            }
            // UInt32 bytesPerFat = (UInt32)sectPerFat * bootRecord.bytesPerSector;
            // UInt32 totalClustersFromFAT = bytesPerFat / 4;

            // totalClustersFromFAT // 1fe00
            // bytesPerSector       // 200
            // totalSectors32       // 7fb80 *** ne aggiunge sectorsPerClusters alla volta... ovvero, aggiunge un Cluster.
            // sectorsPerCluster    // 4        2048bytes
            // sectorsPerTrack      // 20

            // each cluster ranges in size from 4 sectors (2048 bytes) to 64 sectors 
            // 512 Bytes Per Sector
            // 8 Sectors Per Cluster 
            // 4096 Bytes Per Cluster
        }

        public void Write(BinaryWriter bw)
        {
            //byte[] newValume = new byte[] { 1, 2, 3, 4 };
            bw.BaseStream.Seek(_offset, SeekOrigin.Begin);
            bw.Write(jump);
            bw.Write(oem_name);
            bw.Write(bytesPerSector);
            bw.Write(sectorsPerCluster);
            bw.Write(reservedSector);
            bw.Write(numFats);
            bw.Write(rootEntries);
            bw.Write(totalSectors16);
            bw.Write(mediaType);
            bw.Write(sectorsPerFat16);
            bw.Write(sectorsPerTrack);
            bw.Write(numHeads);
            bw.Write(hiddenSectors);
            bw.Write(totalSectors32);

            // WRITE FAT32
            if (FatType == EFatType.FAT32)
            {
                bw.Write(sectorsPerFat32);
                bw.Write(extFlags);
                bw.Write(fsVer);
                bw.Write(rootClust);
                bw.Write(fsInfo);
                bw.Write(bkBootRec);
                bw.Write(reservedFAT32);
                bw.Write(drvNum32);
                bw.Write(reservedFAT32_b);
                bw.Write(bootSig);
                bw.Write(volId);
                bw.Write(volLabel);
                bw.Write(filSysType32);
            }

            // WRITE FAT16
            if (FatType == EFatType.FAT16)
            {
                bw.Write(driveId);
                bw.Write(reserved);
                bw.Write(extendedBootSign);
                bw.Write(volumeSerial);
                bw.Write(volumeName);
                bw.Write(filSysType16);
                bw.Write(execBootCode);
                bw.Write(execSign);
            }
        }
    }


/*    enum EClustersType
    {
        Free = 0x0000,
        Reserved = 0x0001,
        Bad = 0xFFF7,
        Last = 0xFFF8, // -> 0xFFFF
        VeryLast = 0xFFFF
    }*/


    public class Fat
    {
        private Int64 _offset = 0;
        public UInt32[] clustersMap = null;
        public List<UInt32> freeList = new List<UInt32>();
        private EFatType _fatType;


        public void SetOffset(Int64 offset)
        {
            _offset = offset;
        }

        public UInt32 LastBusyCluster
        {
            get
            {
                UInt32 i = (UInt32)(clustersMap.Length - 1);
                while (i > 0 && Clust_IsFree(i))
                    i--;
                if (i <= 0)
                    return 1;
                return i;
            }
        }

        public UInt32 MediaType
        {
            get { return clustersMap[0]; }
        }

        public UInt32 PartitionState
        {
            get { return clustersMap[1]; }
        }

        public Fat(EFatType fatType, UInt32 totDataClusters)
        {
            _fatType = fatType;
            clustersMap = new UInt32[totDataClusters + 2];
            // 0x0000 = free
            // 0x0001 = reserved
            // 0x0002->0xFFEF = cluster
            // 0xFFF0->0xFFF6 = reserved
            // 0xFFF7 = bad cluster
            // 0xFFF8->0xFFFF = last cluster
        }

        public void Resize(UInt32 totDataClusters)
        {
            // Note also that the CountofClusters value is exactly that—the count of data clusters starting at cluster 2. 
            // The maximum valid cluster number for the volume is CountofClusters + 1, and the “count of
            // clusters including the two reserved clusters” is CountofClusters + 2.
            Array.Resize(ref clustersMap, (int)totDataClusters + 2);
        }

        public void Clear()
        {
            UInt32 startClust = 2;
            if (_fatType == EFatType.FAT32)
            {
                startClust = 3;
                if (!Clust_IsEOC(clustersMap[2]))
                    throw new Fat16Exception("RootChain > 1 NOT Supported. Please contact: m.bellino@symbian-toys.com");
            }
            freeList.Clear();
            for (UInt32 i = startClust; i < clustersMap.Length; i++)
            {
                Clust_MarkFree(i);
            }
            freeList.Sort();
        }


        private bool Clust_IsEOC(UInt32 clust)
        {
            UInt32 valueEOC = clust & 0x0FFFFFFF;
            if (_fatType == EFatType.FAT32)
            {
                return (valueEOC >= 0x0FFFFFF7);
            }
            return (valueEOC >= 0xFFF7);
        }

        private void Clust_MarkEOC(UInt32 clust)
        {
            UInt32 valueEOC = 0xFFFF;
            if (_fatType == EFatType.FAT32)
            {
                UInt32 highbits = clustersMap[clust] & 0xF0000000;
                valueEOC = highbits | 0x0FFFFFFF;
            }
            clustersMap[clust] = valueEOC;
        }


        private bool Clust_IsFree(UInt32 clust)
        {
            if (_fatType == EFatType.FAT32)
            {
                UInt32 clustVal = clustersMap[clust] & 0x0FFFFFFF;
                return (clustVal == 0);
            }
            return clustersMap[clust] == 0;
        }

        private void Clust_MarkFree(UInt32 clust)
        {
            UInt32 valueFree = 0;
            if (_fatType == EFatType.FAT32)
            {
                UInt32 highbits = clustersMap[clust] & 0xF0000000;
                valueFree |= highbits;
            }
            clustersMap[clust] = valueFree;
            freeList.Add(clust);
        }



        public UInt32 RemapClusterToBegin(UInt32 busyClust)
        {
            Debug.Assert(freeList.Count > 0);
            UInt32 freeClust = ReserveClusters(1)[0];
            // 200   <->  2
            UInt32 nextBusy = clustersMap[busyClust];
            Debug.Assert(!Clust_IsFree(nextBusy));
            Debug.Assert(Clust_IsFree(freeClust));
            
            // Verifica se fa parte di una catena
            Int32 i=0;
            while (i<clustersMap.Length && clustersMap[i] != busyClust)
                i++;

            if (i < clustersMap.Length)
            {
                // Fa parte di una catena... Aggiorna la catena
                clustersMap[i] = freeClust;
            }
            clustersMap[freeClust] = nextBusy;
            Clust_MarkFree(busyClust);
            freeList.Sort();
            return freeClust;
        }


        public List<UInt32> ReserveClusters(UInt16 qtyClusters)
        {
            List<UInt32> list = new List<UInt32>();
            if (qtyClusters > freeList.Count)
                return list;
            for (int i = 0; i < qtyClusters; i++)
            {
                UInt32 freeClust = freeList[0];
                list.Add(freeClust);
                freeList.RemoveAt(0);
            }
            for (int i = 0; i < list.Count - 1; i++)
            {
                UInt32 busyClust = list[i];
                clustersMap[busyClust] = list[i + 1];
            }
            UInt32 lastClust = list[list.Count-1];
            Clust_MarkEOC(lastClust);
            return list;
        }


        public UInt32 ExtendClustersChain(UInt32 firstCluster)
        {
            List<UInt32> chain = GetClustersChain(firstCluster);
            Debug.Assert(chain.Count > 0);
            List<UInt32> freeClusts = ReserveClusters(1);
            UInt32 newClust = freeClusts[0];
            UInt32 lastClust = chain[chain.Count - 1];
            clustersMap[lastClust] = newClust;
            chain[chain.Count - 1] = newClust;
            return newClust;
        }


        public void DeleteClustersChain(UInt32 firstCluster)
        {
            if (firstCluster < 2)
                return;
            List<UInt32> chain = GetClustersChain(firstCluster);
            foreach (UInt32 clust in chain)
            {
                Clust_MarkFree(clust);
            }
            freeList.Sort();
        }

        public List<UInt32> GetClustersChain(UInt32 firstCluster)
        {
            List<UInt32> list = new List<UInt32>();

            UInt32 nextCluster = firstCluster;
            while ( nextCluster >= 2 && !Clust_IsEOC(nextCluster) )
            {
                list.Add(nextCluster);
                nextCluster = clustersMap[nextCluster];
            }
            return list;
        }

        public void Read(BinaryReader br)
        {
            _offset = br.BaseStream.Position;
            for (UInt32 i = 0; i < clustersMap.Length; i++)
            {
                if (_fatType == EFatType.FAT16)
                    clustersMap[i] = br.ReadUInt16();
                else
                    clustersMap[i] = br.ReadUInt32();
                if (Clust_IsFree(i))
                    freeList.Add((UInt32)i);
            }
            freeList.Sort();
        }

        public void Write(BinaryWriter bw)
        {
            bw.BaseStream.Seek(_offset, SeekOrigin.Begin);
            for (int i = 0; i < clustersMap.Length; i++)
            {
                if (_fatType == EFatType.FAT16)
                    bw.Write((UInt16)clustersMap[i]);
                else
                    bw.Write((UInt32)clustersMap[i]);
            }            
        }
    }


    enum EEntryOrdinal
    {
        LastEntry = 0x00,
        Deleted = 0xE5
    }


    enum EAttribFlag
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        LabelVolume = 0x08,
        Directory = 0x10,
        Archive = 0x20,
        LongFileName = (byte)EAttribFlag.ReadOnly | EAttribFlag.Hidden | EAttribFlag.System | EAttribFlag.LabelVolume
    }


    public class LongEntryChunk
    {
        public byte ordinal = 0;
        public byte[] longFilename1 = new byte[10];
        public byte lfnAttrib = (byte)EAttribFlag.LongFileName;
        public byte reserved;                       // Per WinNT indica se il nome e' maiuscolo o minuscolo
        public byte dosNameChecksum;
        public byte[] longFilename2 = new byte[12];
        public UInt16 lfnDataLocation;              //  00 00
        public byte[] longFilename3 = new byte[4];


        public static LongEntryChunk CreateNew(string filename, byte ord, byte dosCheck)
        {
            LongEntryChunk newChunk = new LongEntryChunk();
            newChunk.LongFileName = filename;
            newChunk.ordinal = ord;
            newChunk.dosNameChecksum = dosCheck;
            return newChunk;
        }

        // 13 caratteri Unicode
        public string LongFileName
        {
            get
            {
                string s = 
                    UnicodeEncoding.Unicode.GetString(longFilename1) +
                    UnicodeEncoding.Unicode.GetString(longFilename2) +
                    UnicodeEncoding.Unicode.GetString(longFilename3);
                int pos0 = s.IndexOf('\0');
                if (pos0 >= 0)
                    s = s.Substring(0, pos0);
                return s;
            }
            private set
            {
                string s = value;
                byte[] data = UnicodeEncoding.Unicode.GetBytes(value);
                int len = data.Length;
                Array.Resize(ref data, 26);
                if (len <= 24)
                {
                    data[len] = 0;
                    data[len+1] = 0;
                    len += 2;
                }
                for (int i = len; i < 26; i++)
                    data[i] = 0xFF;

                // split the value... 10+12+4 \0 0xFFFF 0xFFFF
                Array.Copy(data, 0, longFilename1, 0, longFilename1.Length);
                Array.Copy(data, 10, longFilename2, 0, longFilename2.Length);
                Array.Copy(data, 22, longFilename3, 0, longFilename3.Length);
            }
        }
        

        public void MarkDeleted()
        {
            ordinal = (byte)EEntryOrdinal.Deleted;
        }

        public void Read(BinaryReader br)
        {
            ordinal = br.ReadByte();
            longFilename1 = br.ReadBytes(longFilename1.Length);
            lfnAttrib = br.ReadByte();
            Debug.Assert((lfnAttrib & (byte)EAttribFlag.LongFileName) == (byte)EAttribFlag.LongFileName);

            reserved = br.ReadByte();
            dosNameChecksum = br.ReadByte();

            longFilename2 = br.ReadBytes(longFilename2.Length);
            lfnDataLocation = br.ReadUInt16();
            longFilename3 = br.ReadBytes(longFilename3.Length);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(ordinal);
            bw.Write(longFilename1);
            bw.Write(lfnAttrib);
            bw.Write(reserved);
            bw.Write(dosNameChecksum);
            bw.Write(longFilename2);
            bw.Write(lfnDataLocation);
            bw.Write(longFilename3);
        }
    }


    public class ShortEntry
    {
        public byte[] filename = new byte[11];   // 11 
        public byte attrib;                     // readonly + hidden + system + volumelabel + directory + archive + unused
        public byte reserved;
        public byte[] creationTime = new byte[3]; // 3
        public byte[] creationDate = new byte[2]; // 2
        public byte[] accessDate = new byte[2];   // 2
        public UInt16 dataLocationHi;   // 2
        public byte[] modifTime = new byte[2];    // 2
        public byte[] modifDate = new byte[2];    // 2
        public UInt16 dataLocation;
        public UInt32 dataLength;
        private EFatType _fatType;

        public ShortEntry(EFatType fatType)
        {
            _fatType = fatType;
            filename[0] = 0;
        }


        public UInt32 DataLocation
        {
            get
            {
                UInt32 location = dataLocationHi;
                location <<= 16;
                location += dataLocation;
                return location;
            }
            set
            {
                dataLocationHi = BytesUtils.Hi(value);
                dataLocation = BytesUtils.Lo(value);
            }
        }

        public void MarkLast()
        {
            filename[0] = (byte)EEntryOrdinal.LastEntry;
        }

        public void MarkDeleted()
        {
            filename[0] = (byte)EEntryOrdinal.Deleted;
        }

        // Returns the Checksum
        public byte SetFilename(string filename_ext, int position)
        {
            if (filename_ext != "." && filename_ext != "..")
            {
                string fname = Path.GetFileNameWithoutExtension(filename_ext);
                string ext = Path.GetExtension(filename_ext);
                string ori_fname = fname;
                string ori_ext = ext;

                if (fname.Length > 8)
                    fname = fname.Substring(0, 8);
                if (ext.StartsWith("."))
                    ext = ext.Substring(1);
                if (ext.Length > 3)
                    ext = ext.Substring(0, 3);

                fname = fname.PadRight(8, ' ');
                ext = ext.PadRight(3, ' ');

                filename_ext = fname + ext;
                if (ori_fname.Length > 8 || ori_ext.Length > 4)
                {
                    string s = "" + position;
                    string newName = "";
                    for (int i = 0; i < filename_ext.Length - s.Length - 1; i++)
                        newName += filename_ext[i];
                    newName += '~';
                    newName += s;
                    filename_ext = newName;
                }
            }

            filename_ext = filename_ext.PadRight(11, ' ');
            filename_ext = filename_ext.ToUpper();
            filename = Encoding.ASCII.GetBytes(filename_ext);
            Debug.Assert(filename.Length == 11);

            byte chk = Utils.ComputeDosChk(filename);

            if (filename[0] == 0xE5)
                filename[0] = 0x05;

            return chk;
        }


        public string Filename
        {
            get
            {
                string s = Encoding.ASCII.GetString(filename);
                if (filename[0] == 0x05)
                {
                    Debug.WriteLine("SUBST: 05 -> E5");
                    byte[] newFname = filename.Clone() as byte[];
                    newFname[0] = 0xE5;
                    s = Encoding.ASCII.GetString(filename);
                }
                
                string name = "";
                string ext = "";

                int nameLen = 8;
                if (s.Length < nameLen)
                    nameLen = s.Length;

                if (s.Length > 0)
                    name = s.Substring(0, nameLen).TrimEnd();

                if (s.Length > 8)
                {
                    int extLen = s.Length - 8;
                    ext = s.Substring(8, extLen).Trim();
                    if (ext.Length > 0)
                        ext = "." + ext;
                }
                return name + ext;
            }
        }


        public void Read(BinaryReader br)
        {
            filename = br.ReadBytes(11);            
            attrib = br.ReadByte();
            reserved = br.ReadByte();
            creationTime = br.ReadBytes(3);
            creationDate = br.ReadBytes(2);
            accessDate = br.ReadBytes(2);
            dataLocationHi = br.ReadUInt16();
            modifTime = br.ReadBytes(2);
            modifDate = br.ReadBytes(2);
            dataLocation = br.ReadUInt16();
            dataLength = br.ReadUInt32();
        }

        public void Write(BinaryWriter bw)
        {
            if (filename[0] == 0xe5)
            {
            }
            bw.Write(filename);
            bw.Write(attrib);
            bw.Write(reserved);
            bw.Write(creationTime);
            bw.Write(creationDate);
            bw.Write(accessDate);
            bw.Write(dataLocationHi);
            bw.Write(modifTime);
            bw.Write(modifDate);
            bw.Write(dataLocation);
            bw.Write(dataLength);
        }
    }


    public class Entry : ShortEntry
    {
        public List<LongEntryChunk> chunks = new List<LongEntryChunk>(0);
        private bool _isLast = false;
        private bool _isDeleted = false;
        private Int16 _position = -1;

        public Int16 FirstIndex
        {
            get { return _position; }
        }
        public Int16 LastIndex
        {
            get 
            {
                if (IsDeleted)
                    return _position;
                return (Int16)(_position + chunks.Count); 
            }
        }
        public static int PositionsNeeded(string filename_ext)
        {
            string name = Path.GetFileNameWithoutExtension(filename_ext);
            string ext = Path.GetExtension(filename_ext);

            if (name.Length <= 8 && ext.Length <= 4)
            {
                /*string newName = name.ToUpper();
                string newExt = ext.ToUpper();
                if (newName == name && newExt == ext)
                    return 1;*/
                return 2;
            }
/*                11 => 0
                12 => 1
                13 => 1
                14 => 2
                   ...
                25 => 2
                26 => 2
                27 => 3*/
            string filename = name + ext;
            int chunksNeeded = (int)((filename.Length + 12) / 13);
            return chunksNeeded + 1;
        }


        public static Entry CreateNew(EFatType fatType, Int16 position, UInt32 dataLength, string filename_ext, UInt32 clusterContent, byte attribFlag)
        {
            int posNeededs = PositionsNeeded(filename_ext);

            Entry newEntry = new Entry(position, fatType);
            newEntry.attrib = attribFlag;
            newEntry.DataLocation = clusterContent;     // Conterra' almeno "." e ".."


            byte dosCheck = 0;
            if (posNeededs > 1)
                dosCheck = newEntry.SetFilename(filename_ext, position);
            else
                dosCheck = newEntry.SetFilename(filename_ext, -1);

            newEntry.dataLength = dataLength;

            if (posNeededs <= 1)
                return newEntry;

            // Creates Chunks...
            //                attrib = (byte)(attrib | (byte)(EAttribFlag.LongFileName)); // No, e' settato solo sui chunks

            // Add Chunks...
            newEntry.chunks.Clear();
            int chunksNeeded = posNeededs - 1;
            for (int i = 0; i < chunksNeeded; i++)
            {
                string partFname = filename_ext;
                if (partFname.Length > 13)
                    partFname = filename_ext.Substring(0, 13);
                filename_ext = filename_ext.Substring(partFname.Length);

                byte ordinal = (byte)(i + 1);
                if (i == chunksNeeded - 1)
                    ordinal |= 0x40;
                LongEntryChunk chunk = LongEntryChunk.CreateNew(partFname, ordinal, dosCheck);
                newEntry.chunks.Insert(0, chunk);
            } 
            return newEntry;
        }

        public new void MarkLast()
        {
            base.MarkLast();
            /*foreach (LongEntryChunk chunk in chunks)
            {
                chunk.MarkDeleted();
            }*/
            IsLast = true;
        }

        public new void MarkDeleted()
        {
            Debug.WriteLine("Mark Deleted :"+ Filename);
            base.MarkDeleted();
            foreach (LongEntryChunk chunk in chunks)
            {
                chunk.MarkDeleted();
            }
            IsDeleted = true;
        }

        public bool IsFile
        {
            get 
            {
                if (IsLast) return false;
                return (attrib & (byte)(EAttribFlag.Directory | EAttribFlag.LabelVolume)) == 0; 
            }
        }

        public bool IsDirectory
        {
            get { return (attrib & (byte)(EAttribFlag.Directory | EAttribFlag.LabelVolume)) == (byte)EAttribFlag.Directory; }
        }

        protected bool IsLong
        {
            get { return chunks.Count > 0; }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            private set { _isDeleted = value; }
        }

        public bool IsLast
        {
            get { return _isLast; }
            private set { _isLast = value; }
        }


        public new string Filename
        {
            get
            {
                if (!IsLong)
                {
                    return base.Filename;
                }

                string fname = "";
                foreach (LongEntryChunk chunk in chunks)
                {
                    fname = chunk.LongFileName + fname;
                }
                return fname;
            }
        }


        public Entry(Int16 position, EFatType fatType) : base(fatType)
        {
            _position = position;
        }

        private bool FilledWithSameValue(byte[] array)
        {
            if (array.Length <= 0)
                return false;
            byte value = array[0];
            foreach (byte b in array)
            {
                if (b != value)
                    return false;
            }
            return true;
        }

        public new void Read(BinaryReader br)
        {
            byte[] array = br.ReadBytes(32);
            byte firstByte = array[0];
            byte attr = array[11];

            if (firstByte == (byte)EEntryOrdinal.LastEntry)
            {
                IsLast = true;
                return;
            }
            if ( FilledWithSameValue(array) )
            {
                IsDeleted = true;
            }
            if (firstByte == (byte)EEntryOrdinal.Deleted)
            {
                // Entry Deleted
                IsDeleted = true;
            }
            br.BaseStream.Seek(-32, SeekOrigin.Current);

            // If the first byte is equal to 0x05, the actual filename character for this byte is 0xE5.
            // if the entry is deleted the first byte is changed to 0xE5;
            // if the first byte is 00, entries are considered to have ended for that directory.

            if ((attr & (byte)EAttribFlag.LongFileName) == (byte)EAttribFlag.LongFileName)
            {
                // Long Entry

                if (IsDeleted)
                {
                    // Reads only 1 Chunk
                    chunks = new List<LongEntryChunk>(1);
                    LongEntryChunk chunk = new LongEntryChunk();
                    chunk.Read(br);
                    chunks.Add(chunk);
                    return;
                }
                byte ordinal = firstByte;
                bool lastChunk = ((ordinal & 0x40) > 0);    // 01000000
                int qtaChunks = ordinal & 0x3F;             // 00111111
                chunks = new List<LongEntryChunk>(qtaChunks);

                for (int i = 0; i < chunks.Capacity; i++)
                {
                    // magari i chunks si trovano in 2 clusters differenti...
                    LongEntryChunk chunk = new LongEntryChunk();
                    chunk.Read(br);
                    chunks.Add(chunk);
                }
            }
            base.Read(br);
            
/*            foreach (LongEntryChunk chunk in chunks)
            {
                Debug.Assert(chunk.dosNameChecksum == BytesUtils.ComputeDosChk(base.filename), "Fail Dos Checksum");
            }*/
        }


        public new void Write(BinaryWriter bw)
        {
            bw.BaseStream.Seek(_position * 32, SeekOrigin.Begin);
            foreach (LongEntryChunk chunk in chunks)
            {
                chunk.Write(bw);
            }
            // use unique name!
            base.Write(bw);
        }
    }


    public struct Segment : IComparable
    {
        public Int16 pos;
        public Int16 len;

        #region IComparable Membri di

        public int CompareTo(object obj)
        {
            Segment seg = (Segment) obj;
            if (len != seg.len)
                return len.CompareTo(seg.len);
            return pos.CompareTo(seg.pos);
        }

        #endregion
    }

    public class FreeSegments
    {
        private List<Segment> list = new List<Segment>();

        public void Add(Int16 pos)
        {
            for (int i=0; i<list.Count; i++)
            {
                Segment seg = list[i];
                if (seg.pos + seg.len == pos)
                {
                    seg.len++;
                    list[i] = seg;
                    return;
                }
            }
            Segment newSeg;
            newSeg.pos = pos;
            newSeg.len = 1;
            list.Add(newSeg);
        }

        public Int16 Match(int len)
        {
            foreach (Segment segment in ToArray())
            {
                if (len <= segment.len)
                    return segment.pos;
            }
            return -1;
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public Segment[] ToArray()
        {
            Segment[] array = list.ToArray();
            Array.Sort(array);
            return array;
        }
    }




    public class Fat16 : Logged
    {
        public BootRecord bootRecord;
        public BootRecord bootRecordBkup;
        public List<Fat> fats;
        public List<Entry> rootDir;
        private BinaryReader brFile = null;
        private BinaryWriter bwFile = null;
        private FileStream fs = null;
        private Int64 dataClustersOffset = 0;
        private Int64 rootOffset = 0;
        private string fatImageFilename="";


        private Int16 EntriesPerCluster
        {
            get { return (Int16)(BytesPerCluster / 32); }
        }

        private Int32 BytesPerCluster
        {
            get { return bootRecord.sectorsPerCluster * bootRecord.bytesPerSector; }
        }

        public void OpenImage(string fname)
        {
            fatImageFilename = fname;
            bootRecord = new BootRecord();
            bootRecordBkup = new BootRecord();
            fats = new List<Fat>();
            rootDir = new List<Entry>();
            fs = new FileStream(fname, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            brFile = new BinaryReader(fs);
            bwFile = new BinaryWriter(fs);
            Read();
        }

        public void CloseImage()
        {
            if (fs == null)
                return;
            fatImageFilename = "";
            bwFile.Close();
            brFile.Close();
            fs.Close();
            fats.Clear();
            rootDir.Clear();
        }

        public void Resize(UInt32 newSize)
        {
            bootRecord.Resize(newSize);

            UInt32 sectorsPerFat = bootRecord.sectorsPerFat16;
            if (bootRecord.FatType == EFatType.FAT32)
            {
                sectorsPerFat = bootRecord.sectorsPerFat32;
                bootRecordBkup.Resize(newSize);
            }

            UInt32 bytesPerFat = sectorsPerFat * bootRecord.bytesPerSector;
            brFile.BaseStream.Seek(bootRecord.reservedSector * bootRecord.bytesPerSector, SeekOrigin.Begin);
            Int64 fatOffset = brFile.BaseStream.Position;
            foreach (Fat aFat in fats)
            {
                aFat.Resize(bootRecord.CountOfClusters);
                aFat.SetOffset(fatOffset); // Fix the offset for the 2nd FAT, just in case. 2nd FAT will not be saved on disk.
                fatOffset += bytesPerFat;
            }
            fats[0].Clear();
            // Non posso fare la clear su Fat1 se è stata modificata con nokiacooker perchè ha una mappa sballata.

            rootOffset = bootRecord.reservedSector * bootRecord.bytesPerSector + bytesPerFat * bootRecord.numFats;
            Int64 cluster2Offs = rootOffset + bootRecord.rootEntries * 32;
            dataClustersOffset = cluster2Offs - BytesPerCluster * 2;
            ClearImage();
        }


        private Int64 Cluster2Offset(UInt32 cluster)
        {
            if (cluster < 2)
            {
                return rootOffset;
            }
            return dataClustersOffset + cluster * BytesPerCluster;
        }


        private byte[] ReadClustersChainFromImage(UInt32 firstCluster)
        {
            if (firstCluster < 2)
            {
                if (bootRecord.FatType == EFatType.FAT32)
                {
                    return ReadClustersChainFromImage(bootRecord.rootClust);
                }
                Int64 offset = Cluster2Offset(firstCluster);
                brFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                return brFile.ReadBytes(bootRecord.rootEntries * 32);
            }
            List<UInt32> clustChain = fats[0].GetClustersChain(firstCluster);
            byte[] array = new byte[BytesPerCluster * clustChain.Count];
            int i = 0;
            foreach (UInt32 cluster in clustChain)
            {
                Int64 offset = Cluster2Offset(cluster);
                brFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                byte[] newBytes = brFile.ReadBytes(BytesPerCluster);
                long len = newBytes.Length;
                //Debug.Assert(len > 0);
                // L'ultimo cluster potrebbe contenere meno dati...
                Array.Copy(newBytes, 0, array, i * BytesPerCluster, len);                
                i++;
            }
            return array;
        }

        private void WriteClustersChainToImage(UInt32 firstCluster, byte[] array)
        {
            if (firstCluster < 2)
            {
                if (bootRecord.FatType == EFatType.FAT32)
                {
                    WriteClustersChainToImage(bootRecord.rootClust, array);
                    return;
                }
                Int64 offset = Cluster2Offset(firstCluster);
                bwFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                bwFile.Write(array);
                return;
            }
            List<UInt32> clustChain = fats[0].GetClustersChain(firstCluster);
            int i = 0;
            int oldLen = array.Length;
            Array.Resize(ref array, BytesPerCluster * clustChain.Count);
            foreach (UInt32 cluster in clustChain)
            {
                Int64 offset = Cluster2Offset(cluster);
                bwFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                bwFile.Write(array, i * BytesPerCluster, BytesPerCluster);
                i++;
            }
            Array.Resize(ref array, oldLen);
        }

        #region Reads Contents from Files and Dirs
        public byte[] ReadFileContentFromImage(Entry fileEntry)
        {
            Debug.Assert(fileEntry.IsFile);
            Debug.Assert(!fileEntry.IsDeleted);
            byte[] array = ReadClustersChainFromImage(fileEntry.DataLocation);
            Array.Resize(ref array, (int) fileEntry.dataLength);
            return array;
        }


        private List<Entry> ParseDirectory(byte[] data)
        {
            List<Entry> list = new List<Entry>();
            BinaryReader br = new BinaryReader(new MemoryStream(data));
            bool isLast = false;
            Int16 i = 0;
            while (br.BaseStream.Position < br.BaseStream.Length && ! isLast)
            {
                i = (Int16)(br.BaseStream.Position / 32);
                //Debug.Write("Offset:" + br.BaseStream.Position + " Pos:" + i + "   ");
                Entry entry = new Entry(i, bootRecord.FatType);
                entry.Read(br);
                if (!entry.IsLast)
                {                    
                    //Debug.WriteLine(entry.Filename);
                    list.Add(entry);
                }
                isLast = entry.IsLast;
            }
            // Set to zero all the remaining bytes... so, the Last entry will be detected properly.
            // Array.Clear(data, (int)br.BaseStream.Position, (int) (data.Length - br.BaseStream.Position));
            br.Close();
            return list;
        }


        public List<Entry> ReadDirectoryContentFromImage(Entry dirEntry)
        {
            if (dirEntry == null)
                return rootDir;
            Debug.Assert(dirEntry.IsDirectory);
            Debug.Assert(!dirEntry.IsDeleted);
            byte[] data = ReadClustersChainFromImage(dirEntry.DataLocation);
            return ParseDirectory(data);
        }
        #endregion


        public void DeleteEntryFromImage(Entry parent, Entry entry)
        {
            if (parent != null)
            {
                Debug.Assert(!parent.IsDeleted);
                Debug.Assert(parent.IsDirectory);
            }
            if (entry.Filename == ".")
                return;
            if (entry.Filename == "..")
                return;
            if (entry.IsDeleted)
                return;
            if (entry.IsDirectory)
            {
                // removes all the subdirs...
                List<Entry> childs = ReadDirectoryContentFromImage(entry);
                foreach (Entry aChild in childs)
                {
                    if (!aChild.IsDeleted)
                        DeleteEntryFromImage(entry, aChild);
                }
            }

            UInt32 dirCluster = 0;
            if (parent != null)
                dirCluster = parent.DataLocation;

            // Update to file...
            entry.MarkDeleted();
            byte[] dirData = ReadClustersChainFromImage(dirCluster);
            BinaryWriter bwMem = new BinaryWriter(new MemoryStream(dirData));
            entry.Write(bwMem);
            bwMem.Close();
            WriteClustersChainToImage(dirCluster, dirData);
            
            fats[0].DeleteClustersChain(entry.DataLocation);
            fats[0].Write(bwFile);

//            if (parent == null)
//                rootDir.Remove(entry);
        }


        private FreeSegments FindFreePositionsInDirectory(List<Entry> childs, UInt32 parentCluster)
        {
            List<UInt32> chain = fats[0].GetClustersChain(parentCluster);
            UInt32 maxPosHere = (UInt32)(chain.Count * BytesPerCluster / 32);
            if (parentCluster == 0)
                maxPosHere = bootRecord.rootEntries;

            FreeSegments res = new FreeSegments();

            Int16 pos = -1;
            foreach (Entry aChild in childs)
            {
                if (aChild.IsDeleted)
                    res.Add(aChild.FirstIndex);
                pos = aChild.LastIndex;
            }

            pos++;  // Jump to the Next Free Position...
            while (pos < maxPosHere)
            {
                res.Add(pos);
                pos++;
            }
            return res;
        }


        private List<UInt32> ReserveClustersInImage(UInt16 qty)
        {
            List<UInt32> freeClusters = fats[0].ReserveClusters(qty);
            byte[] array = new byte[BytesPerCluster * freeClusters.Count];
            WriteClustersChainToImage(freeClusters[0], array);
            fats[0].Write(bwFile);
            return freeClusters;
        }


        private UInt32 ExtendClustersChainInImage(UInt32 firstCluster)
        {
            UInt32 newClust = fats[0].ExtendClustersChain(firstCluster);
            byte[] array = new byte[BytesPerCluster];
            WriteClustersChainToImage(newClust, array);
            fats[0].Write(bwFile);
            return newClust;
        }


        private Entry CreateEntry(string filename, UInt32 dataLength, byte attrib, Entry parent, UInt16 neededClusters)
        {
            if (parent != null)
            {
                Debug.Assert(parent.IsDirectory);
                Debug.Assert(!parent.IsDeleted);
            }

            List<Entry> parentChilds = rootDir;
            UInt32 parentCluster = bootRecord.rootClust;
            // add to parent
            if (parent != null)
            {
                parentChilds = ReadDirectoryContentFromImage(parent);
                parentCluster = parent.DataLocation;
            }            

            // Check If Exists in Parent...
            if (FindFileOrDir(filename, parentChilds) != null)
            {
                return null;
            }

            UInt32 contentCluster = 0;
            if (neededClusters > 0)
            {
                List<UInt32> freeClusters = ReserveClustersInImage(neededClusters);
                if (freeClusters.Count <= 0)
                    return null;
                contentCluster = freeClusters[0];
            }

            // Find an unused Position to add the new Entry
            FreeSegments freeSegs = FindFreePositionsInDirectory(parentChilds, parentCluster);

            // Compute space needed for this filename... 
            int lenNeeded = Entry.PositionsNeeded(filename);
            Int16 pos = freeSegs.Match(lenNeeded);
            if (pos < 0)
            {
                // Can't insert the new Entry here...
                // Add a new Cluster... and try again...
                UInt32 newClust = ExtendClustersChainInImage(parentCluster);
                freeSegs = FindFreePositionsInDirectory(parentChilds, parentCluster);
                pos = freeSegs.Match(lenNeeded);
            }

            Debug.Assert(pos >= 0, "No Space Left in Dir");

            // Create the New Entry and Add to the Parent
            Entry newEntry = Entry.CreateNew(bootRecord.FatType, pos, dataLength, filename, contentCluster, (byte)attrib);
//            Entry emptyEntry = new Entry((short)(pos+lenNeeded), bootRecord.FatType);
//            emptyEntry.MarkLast();

            // Debug.WriteLine(newEntry.Filename);
            byte[] parentData = ReadClustersChainFromImage(parentCluster);
            BinaryWriter bwMem1 = new BinaryWriter(new MemoryStream(parentData));
            newEntry.Write(bwMem1);

            // Writes the last entry only if there is enough space.
            // if (bwMem1.BaseStream.Length - bwMem1.BaseStream.Position > 32)
            //     emptyEntry.Write(bwMem1);
            bwMem1.Close();
            WriteClustersChainToImage(parentCluster, parentData);
            if (parent == null)
            {
                rootDir = ParseDirectory(parentData);
                Debug.WriteLine(rootDir.Count + " ****** Add Entry to ROOT: " + filename);
            }

            return newEntry;
        }

/*        public Entry AddFileEntry(string filename, byte[] content)
        {
            return AddFileEntry(filename, content);
        }*/
        
        public Entry AddFileEntry(string filename, byte[] content, Entry parent)
        {
            filename = Path.GetFileName(filename);
            UInt16 neededClusters = (UInt16)((content.Length + BytesPerCluster - 1) / BytesPerCluster);
            Entry newEntry = CreateEntry(filename, (UInt32)content.Length, 0, parent, neededClusters);
            if (newEntry == null)
                return null;
            if (newEntry.DataLocation == 0)
                return newEntry; // Emtpy file

            // Update Content to file...
            WriteClustersChainToImage(newEntry.DataLocation, content);
            return newEntry;
        }


/*        public Entry AddDirectory(string filename)
        {
            return AddDirEntry(filename, null);
        }*/

        public Entry AddDirEntry(string filename, Entry parent)
        {            
            filename = Path.GetFileName(filename);
            Entry newEntry = CreateEntry(filename, 0, (byte)EAttribFlag.Directory, parent, 1);
            if (newEntry == null)
                return null;
            Debug.Assert(newEntry.DataLocation != 0);

            // add to parent
            UInt32 parentCluster = bootRecord.rootClust;
            if (parent != null)
                parentCluster = parent.DataLocation;

            // Create the Content for the New Entry and Add to the New Entry
            Entry dotEntry = Entry.CreateNew(bootRecord.FatType, 0, 0, ".", newEntry.DataLocation, (byte)EAttribFlag.Directory);
            Entry dotDotEntry = Entry.CreateNew(bootRecord.FatType, 1, 0, "..", parentCluster, (byte)EAttribFlag.Directory);

            // Update Directory Content to file...
            byte[] dirData = new byte[BytesPerCluster];
            BinaryWriter bwMem = new BinaryWriter(new MemoryStream(dirData));
            dotEntry.Write(bwMem);
            dotDotEntry.Write(bwMem);
            bwMem.Close();
            WriteClustersChainToImage(newEntry.DataLocation, dirData);

            return newEntry;
        }

        // *** DONE *** Read File and Dir
        // *** DONE *** Remove File and Dir
        // *** DONE *** Save image to file...
        // *** DONE *** Add File and Dir
        // *** DONE *** Long File Names...
        //
        // TODO: Defrag When Click "Save"
        // TODO: Attrib. di ultima modifica file...
        // TODO: Rename File and Dir


        public void CompressFAT()
        {
            // Note also that the CountofClusters value is exactly that—the count of data clusters starting at cluster 2. 
            // The maximum valid cluster number for the volume is CountofClusters + 1, and the “count of
            // clusters including the two reserved clusters” is CountofClusters + 2.

            // When it is full there are
            // CountOfClusters = 31740 these are only the dataClusters... we need to add the 2 init clusters.
            // 0..31741
            // LastBusyCluster = 31741 and it contains the 65535 value
            UInt32 lastBusy = fats[0].LastBusyCluster;
            long maxLen = Cluster2Offset(lastBusy) + BytesPerCluster;
            // E' possibile troncare il file a maxLen...
            bwFile.BaseStream.SetLength(maxLen);

            List<UInt32> freeList = fats[0].freeList;
            if (freeList.Count == 0)
                return;

            // Il primo cluster libero in caso di ottimizzazione della FAT.
            UInt32 bestLastFreeCluster = (UInt32)(fats[0].clustersMap.Length - freeList.Count);
            // TODO: scandisce tutte le entry e le entry che hanno clusters che superano o pareggiano il bestLastFreeCluster dovranno essere riallocate...

/*            while (freeList[0] != bestLastFreeCluster)
            {
                UInt16 lastBusy = fats[0].LastBusyCluster;
                // TODO: Move Cluster from lastBusy to freeList[0]
                UInt16 newClust = fats[0].RemapClusterToBegin(lastBusy);
            }*/
        }


        public void ExtractAllFromImage(List<Entry> entries, string path)
        {
            Directory.CreateDirectory(path);
            foreach (Entry entry in entries)
            {
                if (entry.Filename == ".")
                    continue;
                if (entry.Filename == "..")
                    continue;
                if (entry.IsDeleted)
                    continue;

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(path + entry.Filename);
                    List<Entry> subDir = ReadDirectoryContentFromImage(entry);
                    ExtractAllFromImage(subDir, path + entry.Filename + "\\");
                    subDir.Clear();
                    subDir = null;
                }
                if (entry.IsFile)
                {
//                    Debug.WriteLine(path + entry.Filename);

                    byte[] content = ReadFileContentFromImage(entry);
                    File.WriteAllBytes(path + entry.Filename, content);
                    content = null;
                    // File.WriteAllBytes(path + entry.Filename, content);
                    //GC.WaitForPendingFinalizers();
                    /*FileStream strm = File.Create(path + entry.Filename, 16000);
                    BinaryWriter bw = new BinaryWriter( strm );
                    bw.Write(content);
                    bw.Flush();
                    strm.Flush();
                    strm.Close();
                    bw.Close();
                    strm.Dispose();*/
                    //File.WriteAllText(path + entry.Filename, "prova");
                }
            }
        }


        public UInt32 Maxsize
        {
            get
            {
                // Note also that the CountofClusters value is exactly that—the count of data clusters starting at cluster 2. 
                // The maximum valid cluster number for the volume is CountofClusters + 1, and the “count of
                // clusters including the two reserved clusters” is CountofClusters + 2.

                // When it is full there are
                // CountOfClusters = 31740 these are only the dataClusters... we need to add the 2 init clusters.
                // 0..31741
                // LastBusyCluster = 31741 and it contains the 65535 value
                Int32 lastClustVal = (fats[0].clustersMap.Length - 1);
                Int64 res = Cluster2Offset((UInt32)(lastClustVal)) + BytesPerCluster;
                return (UInt32)res;
/*                UInt32 totalSectors = bootRecord.totalSectors16;
                if (totalSectors == 0)
                    totalSectors = bootRecord.totalSectors32;
                return totalSectors * bootRecord.bytesPerSector;*/
                // UInt32 res = (UInt32)this.fats[0].clustersMap.Length * (UInt32)BytesPerCluster;

                /*UInt32 maxLen = (UInt32) (Cluster2Offset(bootRecord.CountOfClusters) + BytesPerCluster);
                return maxLen;*/
                /*
                UInt32 res = (UInt32)this.fats[0].clustersMap.Length * (UInt32)BytesPerCluster;
                return re;*/
            }
        }

        // TODO: A quanto pare WinImage non gestisce le directory . e .. in versione long
        private void Read()
        {
            bootRecord.Read(brFile);
            brFile.BaseStream.Seek(bootRecord.bkBootRec * bootRecord.bytesPerSector, SeekOrigin.Begin);
            if (bootRecord.FatType == EFatType.FAT32)
                bootRecordBkup.Read(brFile);

            fats.Clear();

            // Investigate about reservedSector
            brFile.BaseStream.Seek(bootRecord.reservedSector * bootRecord.bytesPerSector, SeekOrigin.Begin);

            UInt32 hidden = bootRecord.hiddenSectors;
            // HiddenSectors: this will matchup with the starting sector stated in the partitions' entry in the mbr. it states the number of hidden sectors from the beginning of the drive to the boot record of the partition.
            
            // Position 0x1200
            UInt32 sectPerFat = bootRecord.sectorsPerFat16;
            if (sectPerFat == 0)
                sectPerFat = bootRecord.sectorsPerFat32;
            UInt32 bytesPerFat = sectPerFat * bootRecord.bytesPerSector;

            /*
            The BPB_FATSz16  sectorsPerFat16 (BPB_FATSz32 for FAT32 volumes) value may be bigger than it needs
to be. In other words, there may be totally unused FAT sectors at the end of each FAT in the
FAT region of the volume. For this reason, the last sector of the FAT is always computed
using the CountofClusters + 1 value, never from the BPB_FATSz16/32 value. FAT code
should not make any assumptions about what the contents of these “extra” FAT sectors are.
FAT format code should zero the contents of these extra FAT sectors though.
             */

            if (bootRecord.FatType == EFatType.FAT32)
            {
                if (bytesPerFat < (bootRecord.CountOfClusters + 2) * 4)
                    throw new FwException("Wrong bytesPerFat32 value. This FAT is corrupted, you should trash it.");
            }
            else
            {
                if (bytesPerFat < (bootRecord.CountOfClusters + 2) * 2)
                    throw new FwException("Wrong bytesPerFat16 value. This FAT is corrupted, you should trash it.");
            }

            // Questo calcolo è errato... 
            /* UInt32 totalClustersFromFAT = 0;
            if (bootRecord.FatType == EFatType.FAT16)
                totalClustersFromFAT = bytesPerFat / 2;
            else
                totalClustersFromFAT = bytesPerFat / 4;*/                        
            

            for (int i = 0; i < bootRecord.numFats; i++)
            {
                // each cluster ranges in size from 4 sectors (2048 bytes) to 64 sectors 
                // 512 Bytes Per Sector
                // 4096 Bytes Per Cluster
                // 8 Sectors Per Cluster       

                long oldPos = brFile.BaseStream.Position;
                Fat aFat = new Fat(bootRecord.FatType, bootRecord.CountOfClusters);
                aFat.Read(brFile);

                Int32 skipBytes = (Int32)(bytesPerFat - (brFile.BaseStream.Position - oldPos));
                byte[] skipped = brFile.ReadBytes(skipBytes);

                fats.Add(aFat);
            }

            Debug.WriteLine("Offset1: " + (bootRecord.reservedSector * bootRecord.bytesPerSector + bytesPerFat * bootRecord.numFats));
            Debug.WriteLine("Offset2: " + brFile.BaseStream.Position);
            Debug.Assert(bootRecord.reservedSector * bootRecord.bytesPerSector + bytesPerFat * bootRecord.numFats == brFile.BaseStream.Position);
            
            rootOffset = brFile.BaseStream.Position;
            Int64 cluster2Offs = rootOffset + bootRecord.rootEntries * 32;
            dataClustersOffset = cluster2Offs - BytesPerCluster * 2;
            
            // data clusters starts at cluster 2 right after the root directory.
            
            // RootCluster = 0x14200    -> 0x0200*32 = 0x4000
            // DataCluster = 0x16200
            // Cluster 2 = 0x18200

            byte[] data = null;
            if (bootRecord.FatType == EFatType.FAT16)
            {
                data = brFile.ReadBytes(bootRecord.rootEntries * 32);
            }
            else
            {
                data = ReadClustersChainFromImage(bootRecord.rootClust);
                rootOffset = Cluster2Offset(bootRecord.rootClust);
            }
            rootDir = ParseDirectory(data);
            // Cluster 2 = 0x18200

            //ExtractAllFromImage(rootDir, "C:\\extr\\");
            //AddAllFilesAndFolders("c:\\Test5\\", "d:\\");
            //Entry newDir = AddDirectory("MyTests");
            // data = ReadClustersChainFromImage(0);
            //rootDir = ParseDirectory(data);

            // Entry newFile = AddFile("cs_test.qfi.qfd", File.ReadAllBytes("c:\\cs_test.qfi.qfd"), newDir);
            
            //DeleteEntryFromImage(0, rootDir[6]);
            //DeleteEntry(rootDir[4]);
            //ExtractAllFromImage(rootDir, "c:\\Test\\");/**/
        }

        public Entry FindFileOrDir(string name, List<Entry> childs)
        {
            name = name.ToLower();
            foreach (Entry entry in childs)
            {
                if (entry.IsDeleted)
                    continue;
                if (entry.Filename.ToLower() == name)
                    return entry;
            }
            return null;
        }

        public void ClearImage()
        {
            bootRecord.Write(bwFile);
            if (bootRecord.FatType == EFatType.FAT32)
                bootRecordBkup.Write(bwFile);

            fats[0].Clear();
            fats[0].Write(bwFile);

            // Retrieve current Label, adds a signature...
            // It is needed also for parsing correctly the root on some devices
            string label = "";
            /*
            foreach (Entry entry in rootDir)
            {
                if (((entry.attrib & (byte)EAttribFlag.LabelVolume) == (byte)EAttribFlag.LabelVolume) &&
                    (!entry.IsDeleted))
                {
                    label = entry.Filename;
                }
            }/**/

            byte[] rootData = ReadClustersChainFromImage(0);
            Array.Clear(rootData, 0, rootData.Length);

            if (label != null)
            {
                BinaryWriter bwMem = new BinaryWriter(new MemoryStream(rootData));
                Entry newEntry = Entry.CreateNew(bootRecord.FatType, 0, 0, label, 0, (byte)EAttribFlag.LabelVolume);
                newEntry.Write(bwMem);
                bwMem.Close();
            }
            WriteClustersChainToImage(0, rootData);

            rootDir = ParseDirectory(rootData);

            // Gets the label

            /*
            foreach (Entry entry in rootDir)
            {
                if (((entry.attrib & (byte)EAttribFlag.LabelVolume) == (byte)EAttribFlag.LabelVolume) ||
                    (entry.IsDeleted))
                {
                    // Keep the Label...
                    continue;
                }
                entry.MarkDeleted();

                // Writes the entry to the cluster.
                byte[] dirData = ReadClustersChainFromImage(0);
                BinaryWriter bwMem = new BinaryWriter(new MemoryStream(dirData));
                entry.Write(bwMem);
                bwMem.Close();
                WriteClustersChainToImage(0, dirData);
            }
            */

            CompressFAT();
        }
    }
}
