using System;
using System.Collections.Generic;
using System.Text;
using FuzzyByte.Utils;

namespace Firmware
{
    public class RofsException : FuzzyByteException
    {
        public RofsException(string msg)
            : base(msg)
        {
        }
    }
}
