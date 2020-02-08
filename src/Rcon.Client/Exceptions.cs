using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    public class PacketMalformedException : Exception
    {
        public PacketMalformedException(Exception inner) : base("The provided StreamReader did not contain a valid RCON packet.", inner)
        {

        }
    }
}
