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
        public RconClient(IRconConnection connection, IConnectionStreamOperator streamOperator = default)
        {
            Connection = connection.ThrowIfNull();
        }

        public RconClient(string serverAddress, int port, IConnectionStreamOperator streamOperator = default)
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
        public IRconResponse ExecuteCommand(IRconCommand command)
        {
            if (!Connection.IsOpen)
                Connection.Open();

            var op = Connection.StreamOperator;

            var id = op.WriteMessage(command);

            return op.GetResponse(id);
        }

        /// <inheritdoc/>
        public async Task<IRconResponse> ExecuteCommandAsync(IRconCommand command)
            => await ExecuteCommandAsync(command, default);

        /// <inheritdoc/>
        public async Task<IRconResponse> ExecuteCommandAsync(IRconCommand command, CancellationToken cancellationToken)
        {
            if (!Connection.IsOpen)
                await Connection.OpenAsync(cancellationToken);

            var op = Connection.StreamOperator;

            var id = op.WriteMessage(command);

            return op.GetResponse(id);
        }
    }
}
