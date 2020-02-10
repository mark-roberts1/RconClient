using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace Rcon.Client
{
    /// <summary>
    /// Wraps a <see cref="NetworkStream"/>
    /// </summary>
    public interface IRconStream
    {
        /// <summary>
        /// Returns <see langword="true"/> if there is data available for reading from the stream. <see langword="false"/> if not.
        /// </summary>
        bool DataAvailable { get; }
        /// <summary>
        /// Returns the wrapped stream
        /// </summary>
        Stream GetBaseStream();
    }
}
