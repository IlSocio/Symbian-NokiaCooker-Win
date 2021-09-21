using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FuzzyByte.Utils;


namespace Firmware
{
    public class Content_Sha1
    {
        public byte[] unkn1;            // 12
        public DateTime datetime;       // 4
        public UInt32 maybe_Counter;
        public UInt32 unkn1b;
        public UInt32 signedLen;
        public UInt32 startSha1Offset;
        public UInt32 endSha1Offset;
        public byte[] hash20_sha1_on_data;
        public UInt32 const5;
        public byte[] unkn2;
        public byte[] unknDescr1;
        public UInt32 unkn3;
        public byte[] unkn4;
        public byte[] maybe_signature_rsa_sha1;
        public UInt32 const3;
        public UInt32 length2;
        public UInt32 length3;
        public UInt32 constFF;
        public byte[] unkn5;
        public byte[] unknDescr2;


        public Content_Sha1(byte[] arr)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(arr));
            Read(br);
            br.Close();
        }

        public void Read(BinaryReader br)
        {
            //            string s = BytesUtils.ToHex(unkn1);
            unkn1 = br.ReadBytes(8 + 4);
            UInt32 intVal = br.ReadUInt32();
            datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            datetime = datetime.AddSeconds(intVal);

            maybe_Counter = br.ReadUInt32();
            unkn1b = br.ReadUInt32();
            signedLen = br.ReadUInt32();
            //            unkn1 = br.ReadBytes(12 + 1 + 3 + 1 + 11);
            startSha1Offset = br.ReadUInt32();  // 0x400  = 1024
            endSha1Offset = br.ReadUInt32();    // 0x00627cff  = 6454527 - 1024 = 6453503
            hash20_sha1_on_data = br.ReadBytes(20);
            const5 = br.ReadUInt32();
            unkn2 = br.ReadBytes(284);
            unknDescr1 = br.ReadBytes(8);
            unkn3 = br.ReadUInt32();
            unkn4 = br.ReadBytes(492);
            maybe_signature_rsa_sha1 = br.ReadBytes(16 * 8);
            const3 = br.ReadUInt32();
            length2 = br.ReadUInt32();  // endSha1Offset+1
            length3 = br.ReadUInt32();  // endSha1Offset+1
            constFF = br.ReadUInt32();
            unkn5 = br.ReadBytes(4);
            int toRead = (int)(br.BaseStream.Length - br.BaseStream.Position);
            unknDescr2 = br.ReadBytes(toRead);
            Debug.Assert(const5 == 0x16793A22, "Not Const5");
            Debug.Assert(const3 == 0x31DEEAE8, "Not Const3");
            Debug.Assert(constFF == 0xFFFFFFFF || constFF == 0, "Not FF");
            Debug.Assert(length2 == length3, "Len2 != Len3");
            Debug.Assert(length3 == endSha1Offset + 1, "Len3 != Length+1");
        }

        public void Write(BinaryWriter bw)
        {
        }

        public override string ToString()
        {
            string descr1 = System.Text.ASCIIEncoding.ASCII.GetString(unknDescr1);
            descr1 = descr1.Replace("\0", "");

            string descr2 = BytesUtils.ToString(unknDescr2);
            descr2 = descr2.Replace("\0", "");

            return " maybe_Counter:" + maybe_Counter + Environment.NewLine + " Descr1:" + descr1 + Environment.NewLine + " Descr2:" + descr2 + Environment.NewLine + " Unkn1:" + BytesUtils.ToHex(unkn1) + Environment.NewLine + " Sha1:" + BytesUtils.ToHex(hash20_sha1_on_data) + Environment.NewLine + " Unkn2:" + BytesUtils.ToHex(unkn2) + Environment.NewLine + " maybe_signature:" + BytesUtils.ToHex(maybe_signature_rsa_sha1) + Environment.NewLine + " Length2:" + BytesUtils.ToHex(length2) + Environment.NewLine + " Length3:" + BytesUtils.ToHex(length3) + Environment.NewLine + " StartOffs:" + BytesUtils.ToHex(startSha1Offset) + Environment.NewLine + " EndOffs:" + BytesUtils.ToHex(endSha1Offset);
            //            return "Length:" + BytesUtils.ToHex(length) + " Sha1:" + BytesUtils.ToHex(hash20_sha1) + Environment.NewLine +
            //               " Len2:" + BytesUtils.ToHex(length2) + " Len3:" + BytesUtils.ToHex(length3) + " unkn4:" + descr + " = " + BytesUtils.ToHex(unkn4) + Environment.NewLine +
            //               " Sign:" + BytesUtils.ToHex(maybe_signature_rsa_sha1);
        }
    }/**/


}
