using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    public interface IConnectionStreamOperator : IDisposable
    {
        IRconResponse GetResponse(int commandId);
        int WriteMessage(IRconCommand command);
    }
}
