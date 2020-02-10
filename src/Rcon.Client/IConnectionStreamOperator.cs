using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <summary>
    /// Performs operations on an RCON TCP connection.
    /// </summary>
    public interface IConnectionStreamOperator : IDisposable
    {
        /// <summary>
        /// An optional log action
        /// </summary>
        Action<string> LogAction { get; set; }
        /// <summary>
        /// Blocks the executing thread until a response is received from the RCON server.
        /// </summary>
        /// <param name="commandId">A reference to the command for which to receive a response.</param>
        /// <param name="timeout">An optional timeout in milliseconds. the default is 10 seconds.</param>
        /// <returns>A response from the RCON server.</returns>
        IRconResponse GetResponse(int commandId, int timeout = 10000);
        /// <summary>
        /// Asynchronously receives a response from the RCON server.
        /// </summary>
        /// <param name="commandId">A reference to the command for which to receive a response.</param>
        /// <param name="cancellationToken">A token used to cancel the action.</param>
        /// <returns>A response from the RCON server wrapped in an awaitable Task.</returns>
        Task<IRconResponse> GetResponseAsync(int commandId, CancellationToken cancellationToken);
        /// <summary>
        /// Sends a command to the RCON server.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>A reference to the command sent to the RCON server.</returns>
        int Write(IRconCommand command);
    }
}
