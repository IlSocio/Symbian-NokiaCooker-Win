using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;


namespace Firmware
{
    public class Content_PaSubElem
    {
        public UInt32 offset;
        public UInt32 length;
        public byte[] trash = new byte[12];
        public byte[] descrArray = new byte[12];
        public byte[] content = new byte[0];

        public string Descr
        {
            get
            {
                string descr1 = System.Text.ASCIIEncoding.ASCII.GetString(descrArray);
                descr1 = descr1.Replace("\0", "");
                return descr1;
            }
        }

        public Content_PaSubElem()
        {
        }

        public void Read(BinaryReader br)
        {
            offset = br.ReadUInt32();
            length = br.ReadUInt32();
            trash = br.ReadBytes(12);
            descrArray = br.ReadBytes(12);

            if (offset == 0xFFFFFFFF)
                return;
            long oldPos = br.BaseStream.Position;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            content = br.ReadBytes((int)length);
            br.BaseStream.Seek(oldPos, SeekOrigin.Begin);
        }

        public override string ToString()
        {
            return Descr + " = Offset: " + BytesUtils.ToHex(offset) + " Len: " + BytesUtils.ToHex(length);
        }
    }


    public class Content_PaSubToc
    {
        public List<Content_PaSubElem> toc = new List<Content_PaSubElem>();

        public Content_PaSubToc(byte[] arr)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(arr));
            Read(br);
            br.Close();
        }

        public void Read(BinaryReader br)
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                Content_PaSubElem elem = new Content_PaSubElem();
                elem.Read(br);
                if (elem.offset == 0xffffffff)
                    return;
                toc.Add(elem);
            }
        }


        public void Write(BinaryWriter bw)
        {
        }

        public void ExtractToPath(string path)
        {
            foreach (Content_PaSubElem elem in toc)
            {
                string fname = path + elem.Descr + ".bin";
                File.WriteAllBytes(fname, elem.content);
            }
        }

        public override string ToString()
        {
            string s = "";
            foreach (Content_PaSubElem elem in toc)
            {
                s = elem + Environment.NewLine;
            }
            return s;
        }
    }
}
