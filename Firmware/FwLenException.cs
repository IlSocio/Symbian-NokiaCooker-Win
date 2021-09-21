using System;
using System.Collections.Generic;
using System.Text;

namespace Firmware
{
    public class InvalidFwException : FwException
    {
        public InvalidFwException(string msg)
            : base(msg)
        {
        }
    }
}
