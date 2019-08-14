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

        public async Task<bool> ConnectAsync(int timeout)
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                using (var cts = new CancellationTokenSource(timeout))
                {
                    var task = _tcpListener.AcceptTcpClientAsync();

                    using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
                    {
                        if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                        {
                            throw new OperationCanceledException(cts.Token);
                        }
                        stream = task.Result.GetStream();
                        return true;
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            return false;
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
