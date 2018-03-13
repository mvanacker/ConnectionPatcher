using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionPatcher
{
    public class NoWirelessAdapterException : Exception
    {
        public NoWirelessAdapterException() : base("No wireless adapter detected.") { }
    }
}
