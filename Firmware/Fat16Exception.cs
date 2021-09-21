using System;
using System.Collections.Generic;
using System.Text;
using FuzzyByte.Utils;

namespace Firmware
{
    public class Fat16Exception : FuzzyByteException
    {
        public Fat16Exception(string msg)
            : base(msg)
        {
        }
    }
}
