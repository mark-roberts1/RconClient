using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <inheritdoc/>
    public class RconConnection : IRconConnection
    {
        private readonly string _serverAddress;
        private readonly int _port;
        private readonly TcpClient _tcpClient;

        private bool disposed;

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

                if (StreamOperator != null) StreamOperator.LogAction = value;
            } 
        }

        public RconConnection(string serverAddress, int port)
        {
            _serverAddress = serverAddress.ThrowIfNullOrWhitespace(nameof(serverAddress));
            _port = port;

            _tcpClient = new TcpClient();
            
        }

        public bool IsOpen => _tcpClient.Connected;

        /// <inheritdoc/>
        public IConnectionStreamOperator StreamOperator { get; private set; }

        /// <inheritdoc/>
        public void Close()
        {
            StreamOperator?.Dispose();

            _tcpClient.Close();
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
                Close();
                _tcpClient.Dispose();
                LogAction = null;
                disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Open()
        {
            LogAction?.Invoke($"Connecting to {_serverAddress}:{_port}");

            _tcpClient.Connect(_serverAddress, _port);

            StreamOperator = new ConnectionStreamOperator((RconStream)_tcpClient.GetStream())
            {
                LogAction = LogAction
            };
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            LogAction?.Invoke($"Connecting to {_serverAddress}:{_port}");

            await _tcpClient.ConnectAsync(_serverAddress, _port);

            StreamOperator = new ConnectionStreamOperator((RconStream)_tcpClient.GetStream())
            {
                LogAction = LogAction
            };
        }
    }
}
