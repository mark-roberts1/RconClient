using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <inheritdoc/>
    public class ConnectionStreamOperator : IConnectionStreamOperator
    {
        private bool disposed;
        private static int currentId = 0;
        private static object _idLock = new object();
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _stoppedEvent = new AutoResetEvent(false);
        private readonly Thread _readerThread;
        private readonly ConcurrentDictionary<int, RconResponse> _responses = new ConcurrentDictionary<int, RconResponse>();
        private readonly IRconStream _rconStream;

        private static readonly object _logLock = new object();
        private Action<string> logAction;

        /// <inheritdoc/>
        public Action<string> LogAction
        {
            get
            {
                lock (_logLock)
                {
                    return logAction;
                }
            }
            set
            {
                lock (_logLock)
                {
                    logAction = value;
                }
            }
        }

        /// <inheritdoc/>
        public ConnectionStreamOperator(IRconStream stream)
        {
            stream.ThrowIfNull();

            _rconStream = stream;

            _reader = new BinaryReader(stream.GetBaseStream());
            _writer = new BinaryWriter(stream.GetBaseStream());

            _readerThread = new Thread(CheckForData);
            _readerThread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                _reader.Dispose();
                _writer.Dispose();

                _stopEvent.Set();
                _stoppedEvent.WaitOne();

                disposed = true;
            }
        }

        /// <inheritdoc/>
        public IRconResponse GetResponse(int commandId, int timeout = 10000)
        {
            var response = _responses[commandId];
            int waited = 0;

            while (!response.Complete && waited < timeout)
            {
                Thread.Sleep(1);
                waited += 1;
            }

            _responses.TryRemove(commandId, out response);

            return response;
        }

        /// <inheritdoc/>
        public int Write(IRconCommand command)
        {
            LogAction?.Invoke($"Sending: {command.Text}");

            int id = 0;

            lock (_idLock)
            {
                id = currentId++;
            }

            _responses.TryAdd(id, new RconResponse(id));

            var packet = RconPacket.From(id, command);
            var terminator = RconPacket.CommandTerminator(id);

            _writer.Write(packet.GetBytes());
            _writer.Flush();
            _writer.Write(terminator.GetBytes());
            _writer.Flush();

            return id;
        }

        private void CheckForData()
        {
            while (!_stopEvent.WaitOne(1))
            {
                try
                {
                    if (!_rconStream.DataAvailable) continue;

                    var packet = RconPacket.From(_reader);

                    LogAction?.Invoke($"Received: {packet.Body}");

                    _responses[packet.CommandId].AddPacket(packet);
                }
                catch
                {
                    continue;
                }
            }

            _stoppedEvent.Set();
        }

        /// <inheritdoc/>
        public async Task<IRconResponse> GetResponseAsync(int commandId, CancellationToken cancellationToken)
        {
            var response = _responses[commandId];
            
            while (!response.Complete)
            {
                await Task.Delay(1, cancellationToken);
            }

            _responses.TryRemove(commandId, out response);

            return response;
        }
    }
}
