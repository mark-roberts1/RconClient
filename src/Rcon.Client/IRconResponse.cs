using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rcon.Client
{
    /// <summary>
    /// Represents a response from a connected RCON server
    /// </summary>
    public interface IRconResponse
    {
        /// <summary>
        /// An ID issued during communication with the server
        /// </summary>
        int CommandId { get; }
        /// <summary>
        /// Response from the server
        /// </summary>
        string ResponseText { get; }
    }

    /// <inheritdoc/>
    public sealed class RconResponse : IRconResponse
    {
        private readonly ConcurrentQueue<RconPacket> _packets = new ConcurrentQueue<RconPacket>();
        private readonly object _syncLock = new object();

        private string responseText;
        private bool didSetResponseText;
        private bool complete;

        public int CommandId { get; }

        internal RconResponse(int commandId)
        {
            CommandId = commandId;
        }

        internal void AddPacket(RconPacket packet)
        {
            _packets.Enqueue(packet);

            Complete = packet.IsResponseTerminator;
        }

        internal bool Complete
        {
            get
            {
                lock (_syncLock)
                {
                    return complete;
                }
            }
            private set
            {
                lock (_syncLock)
                {
                    complete = value;
                }
            }
        }

        public string ResponseText
        {
            get
            {
                if (!didSetResponseText)
                {
                    var builder = new StringBuilder();
                    using (var writer = new StringWriter(builder))
                    {
                        while (_packets.TryDequeue(out RconPacket rconPacket) && !rconPacket.IsResponseTerminator)
                        {
                            writer.Write(rconPacket);
                        }

                        writer.Flush();

                        responseText = builder.ToString();
                    }

                    didSetResponseText = true;
                }

                return responseText;
            }
        }
    }
}
