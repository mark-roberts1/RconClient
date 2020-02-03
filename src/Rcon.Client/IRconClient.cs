using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    /// <summary>
    /// Provides access to execute commands over the RCON protocol.
    /// </summary>
    public interface IRconClient : IDisposable
    {
        /// <summary>
        /// Represents an RCON Connection
        /// </summary>
        IRconConnection Connection { get; }
        /// <summary>
        /// Executes a command, and waits for a response.
        /// </summary>
        /// <param name="command">An RCON command</param>
        /// <returns><see cref="IRconResponse"/></returns>
        IRconResponse ExecuteCommand(IRconCommand command);
        /// <summary>
        /// Executes a command asyncronously, and returns a response.
        /// </summary>
        /// <param name="command">An RCON command</param>
        /// <returns><see cref="Task{IRconResponse}"/></returns>
        Task<IRconResponse> ExecuteCommandAsync(IRconCommand command);
        /// <summary>
        /// Executes a command asyncronously, and returns a response.
        /// </summary>
        /// <param name="command">An RCON command</param>
        /// <param name="cancellationToken">A token to cancel the action</param>
        /// <returns><see cref="Task{IRconResponse}"/></returns>
        Task<IRconResponse> ExecuteCommandAsync(IRconCommand command, CancellationToken cancellationToken);
    }
}
