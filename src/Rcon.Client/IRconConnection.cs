using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <summary>
    /// Represents a TCP connection to an RCON server.
    /// </summary>
    public interface IRconConnection : IDisposable
    {
        /// <summary>
        /// Returns <see langword="true"/> if the connection is alive, <see langword="false"/> if not.
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// An operator to perform actions over the connected <see cref="NetworkStream"/>
        /// </summary>
        IConnectionStreamOperator StreamOperator { get; }
        /// <summary>
        /// Opens a TCP connection to the RCON server, and instances the StreamOperator.
        /// </summary>
        void Open();
        /// <summary>
        /// Opens a TCP connection to the RCON server asynchronously, and instances the StreamOperator.
        /// </summary>
        Task OpenAsync();
        /// <summary>
        /// Closes the connection, and disposes the StreamOperator.
        /// </summary>
        void Close();
    }
}
