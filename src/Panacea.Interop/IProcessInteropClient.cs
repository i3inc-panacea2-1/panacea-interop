using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public interface IProcessInteropClient:IProcessInteropChannel
    {
        string ConnectionId { get; }

        Task<bool> ConnectAsync(int timeout);

        void Disconnect();
    }
}
