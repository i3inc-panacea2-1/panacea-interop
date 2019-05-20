using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public class TcpProcessInteropClient : ProcessInteropChannel, IProcessInteropClient
    {
        TcpClient _tcpClient;
        int _port;
        string _hostname;
        public TcpProcessInteropClient(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
            _tcpClient = new TcpClient();
            _tcpClient.NoDelay = true;
            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        }

        public string ConnectionId => _port.ToString();

        public Task<bool> ConnectAsync(int timeout)
        {
            try
            {
                var task = _tcpClient.ConnectAsync(_hostname, _port);
                task.Wait(timeout);
                stream = _tcpClient.GetStream();
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
        }

        public override void Dispose()
        {
            
            Disconnect();
            _tcpClient.Dispose();
            base.Dispose();
        }
    }
}
