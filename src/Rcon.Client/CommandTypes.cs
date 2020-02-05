using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    internal static class CommandTypes
    {
        public const int SERVERDATA_RESPONSE_VALUE = 0;
        public const int SERVERDATA_EXECCOMMAND = 2;
        public const int SERVERDATA_AUTHRESPONSE = 2;
        public const int SERVERDATA_AUTH = 3;
    }
}
