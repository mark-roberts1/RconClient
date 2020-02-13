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
                disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Open()
        {
            _tcpClient.Connect(_serverAddress, _port);

            StreamOperator = new ConnectionStreamOperator((RconStream)_tcpClient.GetStream());
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            await _tcpClient.ConnectAsync(_serverAddress, _port);

            StreamOperator = new ConnectionStreamOperator((RconStream)_tcpClient.GetStream());
        }
    }
}
