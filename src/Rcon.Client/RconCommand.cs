using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    /// <inheritdoc/>
    public class RconCommand : IRconCommand
    {
        internal RconCommand(int commandType, string text)
        {
            if (commandType != CommandTypes.SERVERDATA_AUTH
                && commandType != CommandTypes.SERVERDATA_AUTHRESPONSE
                && commandType != CommandTypes.SERVERDATA_EXECCOMMAND
                && commandType != CommandTypes.SERVERDATA_RESPONSE_VALUE)
            {
                throw new ArgumentException("packetType was not recognized.");
            }

            CommandType = commandType;
            Text = text;
        }

        /// <inheritdoc/>
        public int CommandType { get; }

        /// <inheritdoc/>
        public string Text { get; }

        /// <summary>
        /// Represents an Authentication command
        /// </summary>
        /// <param name="password">Password used for authenticating</param>
        /// <returns><see cref="IRconCommand"/></returns>
        public static IRconCommand Auth(string password)
        {
            return new RconCommand(CommandTypes.SERVERDATA_AUTH, password);
        }

        /// <summary>
        /// Represents a Server command
        /// </summary>
        /// <param name="text">Text of the command</param>
        /// <returns><see cref="IRconCommand"/></returns>
        public static IRconCommand ServerCommand(string text)
        {
            return new RconCommand(CommandTypes.SERVERDATA_EXECCOMMAND, text);
        }

        /// <summary>
        /// Represents an Empty command.
        /// </summary>
        /// <remarks>
        /// This was added in with Health checks in mind. An RCON server will return a packet acknowledging this packet.
        /// </remarks>
        /// <returns><see cref="IRconCommand"/></returns>
        public static IRconCommand Empty()
        {
            return new RconCommand(CommandTypes.SERVERDATA_RESPONSE_VALUE, null);
        }
    }
}
