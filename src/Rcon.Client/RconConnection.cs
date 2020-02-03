using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
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

        public IConnectionStreamOperator StreamOperator { get; private set; }

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

        public virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Close();
                _tcpClient.Dispose();

                disposed = true;
            }
        }

        public void Open()
        {
            _tcpClient.Connect(_serverAddress, _port);
            StreamOperator = new ConnectionStreamOperator(_tcpClient.GetStream());
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            bool connected = false;
            bool cancellationRequested = false;

            var state = new object();

            AsyncCallback callback = (result) =>
            {
                connected = true;
                _tcpClient.EndConnect(result);

                if (cancellationRequested)
                {
                    _tcpClient.Close();
                    return;
                }

                StreamOperator = new ConnectionStreamOperator(_tcpClient.GetStream());
            };

            _tcpClient.BeginConnect(_serverAddress, _port, callback, state);

            try
            {
                while (!connected)
                {
                    await Task.Delay(5, cancellationToken);
                }
            }
            catch (TaskCanceledException ex)
            {
                cancellationRequested = true;
            }
        }
    }
}
