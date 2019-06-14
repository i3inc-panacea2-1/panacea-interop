using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public class TcpProcessInteropServer : ProcessInteropChannel, IProcessInteropClient
    {
        TcpListener _tcpListener;

        public TcpProcessInteropServer(int port)
        {
            _tcpListener = new TcpListener(IPAddress.Loopback, port);
            try
            {
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _tcpListener.Server.NoDelay = true;
                _tcpListener.Start();
            }
            catch {
                Console.WriteLine("");
            }
        }

        public string ConnectionId => (_tcpListener.LocalEndpoint as IPEndPoint)?.Port.ToString();

        public Task<bool> ConnectAsync(int timeout)
        {
            try
            {
                var task = _tcpListener.AcceptTcpClientAsync();
                if (task.Wait(timeout))
                {
                    var client = task.Result;
                    stream = client.GetStream();
                    return Task.FromResult(true);
                }
            }
            catch
            {
                
            }
            return Task.FromResult(false);
        }

        public void Disconnect()
        {
            _tcpListener.Stop();
        }

        public override void Dispose()
        {
            Disconnect();
            base.Dispose();
        }
    }
}
