using System;
using System.Collections.Generic;
using System.Text;
using FuzzyByte.Utils;


namespace Firmware
{
    public class FwException : FuzzyByteException
    {
        public FwException(string msg)
            : base(msg)
        {
        }
    }
}
