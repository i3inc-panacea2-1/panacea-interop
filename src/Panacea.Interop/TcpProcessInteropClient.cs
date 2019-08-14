using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public class TcpProcessInteropClient : ProcessInteropChannel, IProcessInteropClient
    {
        TcpClient _tcpClient;
        int _port;
        public TcpProcessInteropClient(int port)
        {
            _port = port;
            _tcpClient = new TcpClient();
            _tcpClient.NoDelay = true;
            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        }

        public string ConnectionId => _port.ToString();

        public async Task<bool> ConnectAsync(int timeout)
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                using (var cts = new CancellationTokenSource(timeout))
                {
                    var task = _tcpClient.ConnectAsync(IPAddress.Loopback, _port);

                    using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
                    {
                        if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                        {
                            throw new OperationCanceledException(cts.Token);
                        }
                        stream = _tcpClient.GetStream();
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            return _tcpClient.Connected;
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
