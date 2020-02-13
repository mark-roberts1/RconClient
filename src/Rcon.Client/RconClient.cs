using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <inheritdoc/>
    public class RconClient : IRconClient
    {
        private bool disposed;
        
        /// <summary>
        /// Initializes an instance of <see cref="RconClient"/>
        /// </summary>
        /// <param name="connection">An RCON connection</param>
        public RconClient(IRconConnection connection)
        {
            Connection = connection.ThrowIfNull();
        }

        /// <summary>
        /// Initializes an instance of <see cref="RconClient"/>
        /// </summary>
        /// <param name="serverAddress">address of the server (IP or URL)</param>
        /// <param name="port">TCP port</param>
        public RconClient(string serverAddress, int port)
        {
            Connection = new RconConnection(serverAddress, port);
        }

        /// <inheritdoc/>
        public IRconConnection Connection { get; }

        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;

            if (disposing)
            {
                Connection.Dispose();
                disposed = true;
            }
        }

        /// <inheritdoc/>
        public IRconResponse ExecuteCommand(IRconCommand command, int timeout = 10000)
        {
            if (!Connection.IsOpen)
                Connection.Open();

            var op = Connection.StreamOperator;

            var id = op.Write(command);

            return op.GetResponse(id, timeout);
        }

        /// <inheritdoc/>
        public async Task<IRconResponse> ExecuteCommandAsync(IRconCommand command)
            => await ExecuteCommandAsync(command, default);

        /// <inheritdoc/>
        public async Task<IRconResponse> ExecuteCommandAsync(IRconCommand command, CancellationToken cancellationToken)
        {
            if (!Connection.IsOpen)
                Connection.Open();

            var op = Connection.StreamOperator;

            var id = op.Write(command);

            return await op.GetResponseAsync(id, cancellationToken);
        }
    }
}
