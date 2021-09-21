using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace Firmware
{
    public class Utils
    {
        /// <summary>
        /// Split the Path in list of Directories
        /// </summary>
        public static string[] SplitPath(string parentPath)
        {
            if (!parentPath.EndsWith(Path.DirectorySeparatorChar + ""))
                parentPath += Path.DirectorySeparatorChar;

            string pathRoot = Path.GetPathRoot(parentPath);
            if (parentPath.StartsWith(pathRoot))
                parentPath = parentPath.Substring(pathRoot.Length);

            char[] chars = new char[] { Path.DirectorySeparatorChar };
            return parentPath.Split(chars, StringSplitOptions.RemoveEmptyEntries);
        }


        public static byte ComputeDosChk(byte[] name)
        {
            Debug.Assert(name.Length == 11);
            byte sum = 0;
            for (int i = 0; i < name.Length; i++)
            {
                int sum1 = (sum & 1) << 7;
                int sum2 = (sum & 0xfe) >> 1;
                sum = (byte)((sum1 | sum2) + name[i]);
            } 
            return sum;
        }

        public static string CleanDescr(string fname)
        {
            string res = "";
            fname = fname.ToUpper();
            for (int i = 0; i < fname.Length; i++)
                if ((fname[i] >= '0' && fname[i] <= '9') ||
                    (fname[i] >= 'A' && fname[i] <= 'Z'))
                    res += fname[i];
                else
                    res += "_";
            return res;
        }
    }
}
