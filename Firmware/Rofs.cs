
#define ADD_SIGNATURE

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace Firmware
{

    /*

class TRofsDir
	{
	public:
	TUint16	iStructSize;		// Total size of this directory block including padding
	TUint8	padding;
	TUint8	iFirstEntryOffset;	// offset to first entry
	TUint32 iFileBlockAddress;	// address of associated file block
	TUint32	iFileBlockSize;		// size of associated file block
	TRofsEntry	iSubDir;	// first subdir entry (not present if no subdirs)
	};  
     
     
TRofsEntry 
 iStructSize = 2C 
 TUint32[4] iUids 
 iNameOffset = 1E 
 iAtt = 10 
 iFileSize = 00 00 02 1A 
 iFileAddress = 00 00 01 20 
 iAttExtra = FF 
 iNameLen = 07 
 iName[14] = ... 
    */

    // *************************************************************
    // OK... extension e' identica ma con id ROFX

    public class TVersion
    {
        public byte iMajor;
        public byte iMinor;
        public UInt16 iBuild;

        public void Read(BinaryReader br)
        {
            iMajor = br.ReadByte();
            iMinor = br.ReadByte();
            iBuild = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(iMajor);
            bw.Write(iMinor);
            bw.Write(iBuild);
        }
    }


    public class TRofsDirSortInfo
    {
        public UInt16 iSubDirCount;
        public UInt16 iFileCount;
        public List<UInt16> iEntryOffsetDir = new List<ushort>();
        public List<UInt16> iEntryOffsetFiles = new List<ushort>();

        public void Read(BinaryReader br)
        {
            long startPos = br.BaseStream.Position;
            iSubDirCount = br.ReadUInt16();
            iFileCount = br.ReadUInt16();
            for (int i=0; i<iSubDirCount; i++)
                iEntryOffsetDir.Add(br.ReadUInt16());
            for (int i = 0; i < iFileCount; i++)
                iEntryOffsetFiles.Add(br.ReadUInt16());

            long len = br.BaseStream.Position - startPos;
            startPos = (4 - len) & 3;
            if (startPos > 0)
            {
                br.BaseStream.Seek(startPos, SeekOrigin.Current);
            }
        }

        public void Write(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(iSubDirCount);
            bw.Write(iFileCount);
            for (int i = 0; i < iSubDirCount; i++)
                bw.Write(iEntryOffsetDir[i]);
            for (int i = 0; i < iFileCount; i++)
                bw.Write(iEntryOffsetFiles[i]);
            long len = bw.BaseStream.Position - startPos;
            long toPad = (4 - len) & 3;
            for (int i = 0; i < toPad; i++)
                bw.Write((byte)0xFF);
        }
    }




    public class TRofsDir
    {
        public long _offset = 0;
        public UInt16 iStructSize;		// Total size of this directory block including padding
        public byte padding;
        public byte iFirstEntryOffset;	    // offset to first entry
        public UInt32 iFileBlockAddress;	// (***) address of associated file block   (points to TRofsEntry)
        public UInt32 iFileBlockSize;		// size of associated file block
        public List<TRofsEntry> iSubDir = new List<TRofsEntry>(); // first subdir entry (not present if no subdirs)
        public TRofsDirSortInfo iSortInfo = new TRofsDirSortInfo();

        public void Read(BinaryReader br)
        {
            _offset = br.BaseStream.Position;
            long startPos = br.BaseStream.Position;
            iStructSize = br.ReadUInt16();
            Debug.Assert(iStructSize % 2 == 0);
            padding = br.ReadByte();
            iFirstEntryOffset = br.ReadByte();
            iFileBlockAddress = br.ReadUInt32();    // Se 0 allora non contiene files
            iFileBlockSize = br.ReadUInt32();       // Se 0 allora non contiene files

            long toPad = (startPos + iFirstEntryOffset) - br.BaseStream.Position;
            if (toPad != 0)
            {
                br.BaseStream.Seek(toPad, SeekOrigin.Current);
            }


            while (br.BaseStream.Position < iStructSize + startPos)
            {
                TRofsEntry entry = new TRofsEntry();
                entry.Read(br);
                iSubDir.Add(entry);
            }

            long fix_seek = (startPos + iStructSize) - br.BaseStream.Position;
            Debug.Assert(fix_seek <= 0);
            if (fix_seek != 0)
                br.BaseStream.Seek(fix_seek, SeekOrigin.Current);

            iSortInfo.Read(br);

            long len = br.BaseStream.Position - startPos;
            toPad = (4 - len) & 3;
            if (toPad > 0)
            {
                br.BaseStream.Seek(toPad, SeekOrigin.Current);
            }


            /*Debug.WriteLine("Order:");
            i = 0;
            foreach (TRofsEntry subDir in iSubDir)
            {
                long abs_offset = iSortInfo.iEntryOffsetDir[i] * 4 + startPos;
                long loc_offset = iSortInfo.iEntryOffsetDir[i] * 4;
                Debug.WriteLine(abs_offset);
                i++;
            }*/
/*            foreach (TRofsEntry file in _iFiles)
            {
                long abs_offset = iSortInfo.iEntryOffsetFiles[i] * 4 + startPos;
                long loc_offset = iSortInfo.iEntryOffsetFiles[i] * 4 + iStructSize;
                Debug.WriteLine(abs_offset);
                i++;
            }*/
            // sys
            // private
            // resource
            // system
            // data
            // Read files too

            /*if (iFileBlockAddress > 0)
            {
                long oldPos = br.BaseStream.Position;
                br.BaseStream.Seek(iFileBlockAddress, SeekOrigin.Begin);
                Debug.WriteLine("FileOffset:"+(iFileBlockAddress+iFileBlockSize));
                startPos = br.BaseStream.Position;
                while (br.BaseStream.Position < iFileBlockSize + startPos)
                {
                    TRofsEntry entry = new TRofsEntry();
                    entry.Read(br);
                    _iFiles.Add(entry);
                }
                br.BaseStream.Seek(oldPos, SeekOrigin.Begin);
            }/**/
        }

        public void Fix(BinaryWriter bw, uint adjust)
        {
            if (iFileBlockAddress != 0xFFFFFFFF && iFileBlockAddress != 0)
                iFileBlockAddress += adjust;

            foreach (TRofsEntry entry in iSubDir)
            {
                if (entry.iFileAddress != 0xFFFFFFFF && entry.iFileAddress != 0)
                    entry.iFileAddress += adjust;
            }
            bw.BaseStream.Seek(_offset, SeekOrigin.Begin);
            Write(bw);
        }

        public void Write(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(iStructSize);
            bw.Write(padding);
            bw.Write(iFirstEntryOffset);
            bw.Write(iFileBlockAddress);
            bw.Write(iFileBlockSize);

            // Todo Pad?
            long toPad = (4 - iFirstEntryOffset) & 3;
            for (int i = 0; i < toPad; i++)
                bw.Write((byte)0xFF);

            foreach( TRofsEntry entry in iSubDir)
                entry.Write(bw);

            // Se l'ultima entry effettua padding, devo toglierlo...
            long len = (startPos + iStructSize) - bw.BaseStream.Position;
            long fix_seek = (4 - len) & 3;
            bw.BaseStream.Seek(-fix_seek, SeekOrigin.Current);
            
            // Todo Pad?
            iSortInfo.Write(bw);

            // pad ok?
            len = bw.BaseStream.Position - startPos;
            toPad = (4 - len) & 3;
            for (int i = 0; i < toPad; i++)
                bw.Write((byte)0xFF);
        }
    }


    public enum EFileAttribFlag
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        Directory = 0x10
    }


    public class TRofsEntry
    {
        public long _offset = 0;
        public UInt16 iStructSize;	// Total size of entry, header + name + any padding
        public UInt32 iUid1;        // TCheckUid
        public UInt32 iUid2;
        public UInt32 iUid3;
        public UInt32 iUidChecksum;
        public byte iNameOffset;	// offset of iName from start of entry
        public byte iAtt;			// standard file attributes 
        public UInt32 iFileSize;		// real size of file in bytes (may be different from size in image)
        // for subdirectories this is the total size of the directory
        // block entry excluding padding
        public UInt32 iFileAddress;	// (***) address in image of file start  (point to TRofsDir or to fileContents)
        public byte iAttExtra;		// extra ROFS attributes (these are inverted so 0 = enabled)
        public byte iNameLength = 0;	// length of iName
        public string iName = "";
        //public byte[] _fileContent = new byte[0];
        //public TRofsDir _dirContent = new TRofsDir();

        public void Read(BinaryReader br)
        {
            _offset = br.BaseStream.Position;
            long startPos = br.BaseStream.Position;
            iStructSize = br.ReadUInt16();
            iUid1 = br.ReadUInt32();
            iUid2 = br.ReadUInt32();
            iUid3 = br.ReadUInt32();
            iUidChecksum = br.ReadUInt32();
            iNameOffset = br.ReadByte();
            iAtt = br.ReadByte();
            if (iAtt != 0x10 && iAtt != 0x01)
            { 
                // KEntryAttDir
                // KEntryAttReadOnly
                // KEntryAttHidden
                // KEntryAttSystem
            }
            // ((iAtt & 0x10) > 0) Directory
            // ((iAtt & 0x80) > 0) Eseguibile
            iFileSize = br.ReadUInt32();
            iFileAddress = br.ReadUInt32();         // TODO: Per nascondere un file che si trova in una immagine ROFX bisogna impostare questo campo a zero!
            iAttExtra = br.ReadByte();

            /*
        case 'u':
			iAttExtra |= (KEntryAttUnique >> 23);	// '1' represents disabled in iAttExtra
			break;
		case 'U':
			iAttExtra &= ~(KEntryAttUnique >> 23);	// '0' represent enabled in iAttExtra
			break;
            */

            Debug.Assert(iAttExtra == 0xFF || iAttExtra == 0xFD);        // Features.dat su N8 ha iAttExtra = 0xFD equivale a exAttrib=U
            iNameLength = br.ReadByte();
            iName = "";
            if (iNameLength > 0)
                iName = Encoding.Unicode.GetString( br.ReadBytes(iNameLength * 2) );

            long toPad = (startPos + iStructSize) - br.BaseStream.Position;
            if (toPad != 0)
                br.BaseStream.Seek(toPad, SeekOrigin.Current);


            /*if ((iAtt & 0x10) > 0)
            {
                // Read dir contents too
                _dirContent.Read(br);
            }
            else
            {
                // Read file contents too
                long oldPos = br.BaseStream.Position;
                br.BaseStream.Seek(iFileAddress, SeekOrigin.Begin);
                Debug.WriteLine("ContentOffsetEnd:" + (iFileAddress + iFileSize));
                _fileContent = br.ReadBytes((int)iFileSize);
                br.BaseStream.Seek(oldPos, SeekOrigin.Begin);
            }*/
        }

        public void FixToCore(BinaryWriter bw, TRofsEntry coreEntry)
        {
            bw.BaseStream.Seek(_offset, SeekOrigin.Begin);
            iFileAddress = coreEntry.iFileAddress;
            iFileSize = coreEntry.iFileSize;
            iAtt = coreEntry.iAtt;
            iAttExtra = coreEntry.iAttExtra;
            iUid1 = coreEntry.iUid1;
            iUid2 = coreEntry.iUid2;
            iUid3 = coreEntry.iUid3;
            iUidChecksum = coreEntry.iUidChecksum;
            Write(bw);
        }

        public void Fix(BinaryWriter bw, uint adjust)
        {
            bw.BaseStream.Seek(_offset, SeekOrigin.Begin);
            if (iFileAddress != 0xFFFFFFFF && iFileAddress != 0)
                iFileAddress += adjust;
            Write(bw);
        }

        public void Write(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(iStructSize);
            bw.Write(iUid1);
            bw.Write(iUid2);
            bw.Write(iUid3);
            bw.Write(iUidChecksum);
            bw.Write(iNameOffset);
            bw.Write(iAtt);
            bw.Write(iFileSize);
            bw.Write(iFileAddress);
            bw.Write(iAttExtra);
            bw.Write(iNameLength);
            byte[] nameBytes = Encoding.Unicode.GetBytes(iName);
            if (iNameLength > 0)
                bw.Write(nameBytes);

            // pad ok?
            long len = bw.BaseStream.Position - startPos;
            long toPad = (4 - len) & 3;
            for (int i = 0; i < toPad; i++)
                bw.Write((byte)0xFF);
        }
    }


    public class TContent
    {
        public byte[] data = new byte[0];

        public void Read(BinaryReader br, TRofsEntry fileEntry, UInt32 adjust)
        {
            UInt32 fileAddress = fileEntry.iFileAddress;
            if (fileAddress == 0)           // Il file e' stato nascosto dalla ROFX
                return;
            if (fileAddress == 0xFFFFFFFF)  // Il file e' stato nascosto dalla ROFS
                return;                 
            if (fileAddress < adjust)       // Il file e' un riferimento alla ROFS del CORE...
                return;

            fileAddress -= adjust;
            br.BaseStream.Seek(fileAddress, SeekOrigin.Begin);
            long startPos = br.BaseStream.Position;
            data = br.ReadBytes((int)fileEntry.iFileSize);
            long len = br.BaseStream.Position - startPos;
            long toPad = (4 - len) & 3;
            if (toPad > 0)
                br.BaseStream.Seek(toPad, SeekOrigin.Current);
        }
        public void Write(BinaryWriter bw, TRofsEntry fileEntry, UInt32 adjust)
        {
            UInt32 fileAddress = fileEntry.iFileAddress;
            if (fileAddress == 0)           // Il file e' stato nascosto dalla ROFX
                return;
            if (fileAddress == 0xFFFFFFFF)  // Il file e' stato nascosto dalla ROFS
                return;
            if (fileAddress < adjust)       // Il file e' un riferimento alla ROFS del CORE...
                return;

            fileAddress -= adjust;
            bw.BaseStream.Seek(fileAddress, SeekOrigin.Begin);
            long startPos = bw.BaseStream.Position;
            bw.Write(data);
            long len = bw.BaseStream.Position - startPos;
            long toPad = (4 - len) & 3;
            for (int i = 0; i < toPad; i++)
                bw.Write((byte)0xFF);
        }
    }

    public class TRofsHeader
    {
        public byte[] iIdentifier = new byte[4];
        public byte iHeaderSize;
        public byte iReserved;
        public UInt16 iRofsFormatVersion;
        public UInt32 iDirTreeOffset;           
        public UInt32 iDirTreeSize;             
        public UInt32 iDirFileEntriesOffset;    
        public UInt32 iDirFileEntriesSize;
        public Int64 iTime;
        public TVersion iImageVersion = new TVersion();
        public UInt32 iImageSize;
        public UInt32 iCheckSum;
        public UInt32 iMaxImageSize;            // rofssize=0x40000000
        public UInt32 _iAdjustment = 0;

        public void Read(BinaryReader br)
        {
            iIdentifier = br.ReadBytes(4);
            iHeaderSize = br.ReadByte();
            iReserved = br.ReadByte();
            iRofsFormatVersion = br.ReadUInt16();
            iDirTreeOffset = br.ReadUInt32();
            iDirTreeSize = br.ReadUInt32();
            iDirFileEntriesOffset = br.ReadUInt32();
            iDirFileEntriesSize = br.ReadUInt32();
            iTime = br.ReadInt64();
//            Int64 swap = 0x321aa2b85b18e000;  //0x00e185b8a21a3200
            //Int64 swap = BytesUtils.SwapBytes(iTime);
//            DateTime dt = new DateTime(swap);
//            string descr = dt.ToShortDateString() + "  " + dt.ToShortTimeString();
#if ADD_SIGNATURE
            iTime = 0x3533434E;     // NC35 Signature
#endif
            iImageVersion.Read(br);
            iImageSize = br.ReadUInt32();
            iCheckSum = br.ReadUInt32();
            iMaxImageSize = br.ReadUInt32();

            // Fix Header for ROFx
            if (iIdentifier[0] != 0x52 || iIdentifier[1] != 0x4F || iIdentifier[2] != 0x46)
                throw new RofsException("IMAGE is NOT a ROFS file!");

            if (iIdentifier[3] != 0x78 && iIdentifier[3] != 0x53)
                throw new RofsException("IMAGE is NOT a ROFS file!");

            if (iIdentifier[3] == 0x78) // ROFx
            {
                _iAdjustment = (UInt32)(iDirTreeOffset - br.BaseStream.Position); 
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(iIdentifier);
            bw.Write(iHeaderSize);
            bw.Write(iReserved);
            bw.Write(iRofsFormatVersion);
            bw.Write(iDirTreeOffset);
            bw.Write(iDirTreeSize);
            bw.Write(iDirFileEntriesOffset);
            bw.Write(iDirFileEntriesSize);
            bw.Write(iTime);
            iImageVersion.Write(bw);
            bw.Write(iImageSize);
            bw.Write(iCheckSum);
            bw.Write(iMaxImageSize);
        }
    }

}
