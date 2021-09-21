using System;
using System.Collections.Generic;
using System.Text;
using FuzzyByte.Utils;

namespace Firmware
{
    public class CRCException : FuzzyByteException
    {
        public CRCException(string msg)
            : base(msg)
        {
        }
    }
}
