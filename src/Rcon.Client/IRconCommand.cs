using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    /// <summary>
    /// A command to issue to an RCON server.
    /// </summary>
    public interface IRconCommand
    {
        /// <summary>
        /// The type of command.
        /// </summary>
        /// <remarks>
        /// You can use the factory methods on <see cref="RconCommand"/> for ease of use, but these are the valid types:
        /// SERVERDATA_RESPONSE_VALUE = 0
        /// SERVERDATA_EXECCOMMAND = 2
        /// SERVERDATA_AUTHRESPONSE = 2
        /// SERVERDATA_AUTH = 3
        /// </remarks>
        int CommandType { get; }
        /// <summary>
        /// The text representation of the command.
        /// </summary>
        string Text { get; }
    }
}
