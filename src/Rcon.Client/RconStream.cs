using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Rcon.Client
{
    /// <inheritdoc/>
    public class RconStream : IRconStream
    {
        private readonly NetworkStream BaseStream;
        
        public RconStream(NetworkStream stream)
        {
            BaseStream = stream;
        }

        /// <inheritdoc/>
        public bool DataAvailable => BaseStream.DataAvailable;

        public static implicit operator RconStream(NetworkStream stream)
        {
            return new RconStream(stream);
        }

        /// <inheritdoc/>
        public Stream GetBaseStream() => BaseStream;
    }
}
