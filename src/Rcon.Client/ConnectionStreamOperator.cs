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
        private int messageCounter;
        private static readonly byte[] PADDING = new byte[] { 0x0, 0x0 };
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _stoppedEvent = new AutoResetEvent(false);
        private readonly Thread _readerThread;
        private readonly ConcurrentQueue<(int MessageCounter, byte[] Data)> _readMessages = new ConcurrentQueue<(int MessageCounter, byte[] Data)>();
        private readonly ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim();
        public ConnectionStreamOperator(NetworkStream stream)
        {
            stream.ThrowIfNull();
            messageCounter = 0;
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
            _streamLock.EnterReadLock();
            var allBytes = new List<byte>();

            try
            {
                (int MessageCounter, byte[] Data) msg = (default, default);
                
                int loops = 0;

                while (_readMessages.TryDequeue(out msg) && loops < 100)
                {
                    loops++;

                    if (msg.MessageCounter != commandId)
                    {
                        _readMessages.Enqueue(msg);
                        continue;
                    }

                    allBytes.AddRange(msg.Data);
                }
            }
            finally
            {
                _streamLock.ExitReadLock();
            }

            return new RconResponse(allBytes.ToArray());
        }

        public int WriteMessage(IRconCommand command)
        {
            _streamLock.EnterWriteLock();
            
            int id = messageCounter++;

            try
            {
                switch (command.CommandType)
                {
                    case CommandType.Command:
                        SendCommand(command.Text, id);
                        break;
                    case CommandType.Login:
                        SendLogin(command.Text, id);
                        break;
                    default:
                        throw new InvalidOperationException("Cannot handle this type of command.");
                }
            }
            finally
            {
                _streamLock.ExitWriteLock();
            }

            return id;
        }

        private void SendCommand(string text, int commandId)
        {
            var msg = new List<byte>();
            msg.AddRange(BitConverter.GetBytes(10 + Encoding.UTF8.GetByteCount(text)));
            msg.AddRange(BitConverter.GetBytes(commandId));
            msg.AddRange(BitConverter.GetBytes((int)CommandType.Command));
            msg.AddRange(Encoding.UTF8.GetBytes(text));
            msg.AddRange(PADDING);

            _writer.Write(msg.ToArray());
            _writer.Flush();
        }

        private void SendLogin(string password, int commandId)
        {
            var msg = new List<byte>();
            msg.AddRange(BitConverter.GetBytes(10 + Encoding.UTF8.GetByteCount(password)));
            msg.AddRange(BitConverter.GetBytes(commandId));
            msg.AddRange(BitConverter.GetBytes((int)CommandType.Login));
            msg.AddRange(ASCIIEncoding.UTF8.GetBytes(password));
            msg.AddRange(PADDING);

            _writer.Write(msg.ToArray());
            _writer.Flush();
        }

        private void CheckForData()
        {
            while (!_stopEvent.WaitOne(1))
            {
                try
                {
                    var len = _reader.ReadInt32();
                    var messageId = _reader.ReadInt32();
                    var type = _reader.ReadInt32();
                    var data = len > 10 ? _reader.ReadBytes(len - 10) : new byte[] { };
                    var pad = _reader.ReadBytes(2);

                    _readMessages.Enqueue((messageId, data));
                }
                catch
                {
                    continue;
                }
            }

            _stoppedEvent.Set();
        }
    }
}
