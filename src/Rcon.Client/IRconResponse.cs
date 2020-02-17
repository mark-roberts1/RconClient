using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

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
        private string responseText;
        private bool didSetResponseText;
        private bool complete;
        private readonly ReaderWriterLockSlim _completedLock = new ReaderWriterLockSlim();

        public int CommandId { get; }

        internal RconResponse(int commandId)
        {
            CommandId = commandId;
        }

        internal void AddPacket(RconPacket packet)
        {
            _packets.Enqueue(packet);

            Complete = true;
        }

        internal bool Complete
        {
            get
            {
                if (!_completedLock.TryEnterReadLock(10)) return false;

                try
                {
                    return complete;
                }
                finally
                {
                    _completedLock.ExitReadLock();
                }
            }
            private set
            {
                while (!_completedLock.TryEnterWriteLock(1)) { }

                try
                {
                    complete = value;
                }
                finally
                {
                    _completedLock.ExitWriteLock();
                }
            }
        }

        /// <inheritdoc/>
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
                            writer.Write(rconPacket.Body);
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
