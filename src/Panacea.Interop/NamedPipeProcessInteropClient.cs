using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Panacea.Interop
{
    public class NamedPipeProcessInteropClient: ProcessInteropChannel, IProcessInteropClient
    {
        String _pipeName;
        NamedPipeClientStream _namedPipeClient;
        Timer _timer = new Timer(1000);

        public string ConnectionId => _pipeName;

        public NamedPipeProcessInteropClient(string pipeName)
        {
            _pipeName = pipeName;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Publish("ping");
        }

        public async Task<bool> ConnectAsync(int timeout)
        {
            _namedPipeClient = new NamedPipeClientStream(".",_pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await _namedPipeClient.ConnectAsync(timeout);
                stream = _namedPipeClient;
                _timer.Start();
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
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
            _namedPipeClient.Close();
            _namedPipeClient.Dispose();
            base.Dispose();
        }

        public void Disconnect()
        {
            stream.Close();
        }
    }
}
