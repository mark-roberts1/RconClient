using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    public interface IRconCommand
    {
        CommandType CommandType { get; }
        string Text { get; }
    }
}
