using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcon.Client
{
    public interface IRconConnection : IDisposable
    {
        bool IsOpen { get; }

        IConnectionStreamOperator StreamOperator { get; }
        void Open();
        Task OpenAsync(CancellationToken cancellationToken);
        void Close();
    }
}
