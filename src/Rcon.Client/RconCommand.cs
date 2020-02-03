using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    public class RconCommand : IRconCommand
    {
        public RconCommand(CommandType commandType, string text)
        {
            CommandType = commandType;
            Text = text;
        }

        public CommandType CommandType { get; }
        public string Text { get; }

        public static RconCommand Login(string password)
        {
            return new RconCommand(CommandType.Login, password);
        }

        public static RconCommand ServerCommand(string text)
        {
            return new RconCommand(CommandType.Command, text);
        }
    }
}
