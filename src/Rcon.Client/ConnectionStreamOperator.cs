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
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _stoppedEvent = new AutoResetEvent(false);
        private readonly ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim();
        private readonly Thread _readerThread;
        private readonly ConcurrentDictionary<int, RconResponse> _responses = new ConcurrentDictionary<int, RconResponse>();
        private readonly IRconStream _rconStream;

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
            int id = 0;

            if (!_streamLock.TryEnterWriteLock(5000)) throw new TimeoutException("Timeout of 5 seconds elapsed before a write lock could be acquired for sending command.");

            try
            {
                currentId++;

                id = currentId;

                _responses.TryAdd(id, new RconResponse(id));

                var packet = RconPacket.From(id, command);
                var terminator = RconPacket.CommandTerminator(id);

                _writer.Write(packet.GetBytes());
                _writer.Flush();
                _writer.Write(terminator.GetBytes());
                _writer.Flush();
            }
            finally
            {
                _streamLock.ExitWriteLock();
            }

            return id;
        }

        private void CheckForData()
        {
            while (!_stopEvent.WaitOne(1))
            {
                bool lockAcquired = false;

                try
                {
                    if (!_rconStream.DataAvailable) continue;

                    lockAcquired = _streamLock.TryEnterReadLock(10);

                    if (!lockAcquired) continue;

                    var packet = RconPacket.From(_reader);

                    _responses[packet.CommandId].AddPacket(packet);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    if (lockAcquired)
                        _streamLock.ExitReadLock();
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
