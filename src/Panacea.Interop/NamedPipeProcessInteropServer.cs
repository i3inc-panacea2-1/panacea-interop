using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public class NamedPipeProcessInteropServer: ProcessInteropChannel, IProcessInteropClient
    {
        string _pipeName;
        NamedPipeServerStream _pipe;
        System.Timers.Timer _timer = new System.Timers.Timer(1000);

        public string ConnectionId => _pipeName;

        public NamedPipeProcessInteropServer(string pipeName)
        {
            _pipeName = pipeName;
            var _pipeSecurity = new PipeSecurity();
            var psEveryone = new PipeAccessRule("Everyone", PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
            _pipeSecurity.AddAccessRule(psEveryone);
            _pipe = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 100, 100, _pipeSecurity);
            stream = _pipe;
            _timer.Elapsed += _timer_Elapsed;
        }

        public async Task<bool> ConnectAsync(int timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            try
            {
                await _pipe.WaitForConnectionAsync(cts.Token);
                
                _timer.Start();
                return true;
            }
            catch (OperationCanceledException)
            {
            }
            return false;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Publish("ping");
        }

        protected override void OnClose()
        {
            base.OnClose();
            _timer.Stop();
        }

        public override void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
            _pipe.Disconnect();
            _pipe.Dispose();
            base.Dispose();
        }

        public void Disconnect()
        {
            _pipe.Disconnect();
        }
    }
}
