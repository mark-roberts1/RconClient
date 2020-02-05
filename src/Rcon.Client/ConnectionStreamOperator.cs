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
    public class ConnectionStreamOperator : IConnectionStreamOperator
    {
        private bool disposed;
        private static int currentId = 0;
        private static readonly byte[] PADDING = new byte[] { 0x0, 0x0 };
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private readonly ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim();
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _stoppedEvent = new AutoResetEvent(false);
        private readonly Thread _readerThread;
        private readonly ConcurrentDictionary<int, RconResponse> _responses = new ConcurrentDictionary<int, RconResponse>();

        public ConnectionStreamOperator(NetworkStream stream)
        {
            stream.ThrowIfNull();
            
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);
            
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
                _stopEvent.Set();
                _stoppedEvent.WaitOne();
                
                _reader.Dispose();
                _writer.Dispose();
                
                disposed = true;
            }
        }

        public IRconResponse GetResponse(int commandId)
        {
            var response = _responses[commandId];

            while (!response.Complete)
            {
                Thread.Sleep(5);
            }

            _responses.TryRemove(commandId, out response);

            return response;
        }

        public int WriteMessage(IRconCommand command)
        {
            _streamLock.EnterWriteLock();

            int id = currentId++;

            _responses.TryAdd(id, new RconResponse(id));

            try
            {
                var packet = RconPacket.From(id, command);
                var terminator = RconPacket.CommandTerminator(id);

                _writer.Write(packet.GetBytes());
                _writer.Flush();
                _writer.Write(terminator.GetBytes());
                _writer.Flush();

                return id;
            }
            finally
            {
                _streamLock.ExitWriteLock();
            }
        }

        private void CheckForData()
        {
            while (!_stopEvent.WaitOne(1))
            {
                try
                {
                    var packet = RconPacket.From(_reader);

                    _responses[packet.CommandId].AddPacket(packet);
                }
                catch
                {
                    continue;
                }
            }

            _stoppedEvent.Set();
        }

        public Task<IRconResponse> GetResponseAsync(int commandId)
            => GetResponseAsync(commandId, default);

        public async Task<IRconResponse> GetResponseAsync(int commandId, CancellationToken cancellationToken)
        {
            var response = _responses[commandId];

            while (!response.Complete)
            {
                await Task.Delay(5, cancellationToken);
            }

            _responses.TryRemove(commandId, out response);

            return response;
        }
    }
}
