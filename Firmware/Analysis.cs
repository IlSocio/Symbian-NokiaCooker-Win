using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using FuzzyByte.Utils;

namespace Firmware
{
    public class Analysis
    {
        public static string ComputeSHA1(byte[] buffer)
        {
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", " ");
            return hash;
        }

        public static string ComputeMD5(byte[] buffer)
        {
            MD5CryptoServiceProvider cryptoTransformMD5 = new MD5CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformMD5.ComputeHash(buffer)).Replace("-", " ");
            return hash;
        }

        public static UInt16 ComputeChecksum16(string fname)
        {            
            byte[] b = File.ReadAllBytes(fname);
            return ComputeChecksum16(b);
        }

        public static UInt16 ComputeChecksum16(byte[] array)
        {
            UInt16 res1 = 0;
            UInt16 res2 = 0;
            int i = 0;
            foreach (UInt16 b in array)
            {
                i++;
                if (i % 2 == 0)
                    res1 = (UInt16)(res1 ^ b);
                else
                    res2 = (UInt16)(res2 ^ b);
            }
            res1 = (UInt16)(res1 << 8);
            res1 = (UInt16)(res1 | res2);
            return res1;
        }


        // Compute the Checksum as 0xFF - const01 - contentType - blockSize - .... 
        // Credits for this Checksum goes to the PNHT
        public static byte ComputeChecksum8(byte const01, TBlockType blockType, byte blockSize, byte[] array)
        {
            // TODO: get all data from block... Using Externalize...
            byte computedCrc8 = (byte) (const01 + (byte)blockType + blockSize);
            foreach (byte b in array)
                computedCrc8+=b;
            computedCrc8 = (byte)(0xFF - computedCrc8);
            return computedCrc8;
        }

        public static bool Contains(byte[] array, int value)
        {
            foreach (byte b in array)
                if (b == value)
                    return true;
            return false;
        }

        public static int CountNotZeros(byte[] array)
        {
            int count = 0;
            foreach (byte b in array)
                if (b != 0) count++;
            return count;
        }

        public static bool AreEquals(byte[] array)
        {
            foreach (byte b in array)
            {
                if (array[0] != b)
                    return false;
            }
            return true;
        }

        public static int RangeValues(int[] freqs)
        {
            int qtaDiff = 0;
            foreach (int freq in freqs)
            {
                if (freq > 0)
                    qtaDiff++;
            }
            return qtaDiff;
        }

        public static int[] GetFreqs(byte[] array)
        {
            int[] res = new int[256];
            foreach (byte b in array)
                res[b]++;
            return res;
        }

        public static int[] GetFreqs(string fname)
        {
            byte[] b = File.ReadAllBytes(fname);
            return GetFreqs(b);
        }

        public static string Freq2String(int[] freqs)
        {
            string res = "";
            for (int i = 0; i < freqs.Length; i++)
            {
                res = res + BytesUtils.ToHex((byte)i) + "=" + freqs[i] + "\r\n";
            }
            return res;
        }


    }
}
