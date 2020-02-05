using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    public interface IRconCommand
    {
        int CommandType { get; }
        string Text { get; }
    }
}
