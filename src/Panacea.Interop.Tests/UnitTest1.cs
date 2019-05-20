using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panacea.Interop;

namespace Panacea.Interop.Tests
{
    [TestClass]
    public class UnitTest1:IDisposable
    {
        TcpProcessInteropClient _client = new TcpProcessInteropClient("localhost", 1800);
        TcpProcessInteropServer _server = new TcpProcessInteropServer(1800);

        public UnitTest1()
        {

        }

        Task Run(Func<Task> action)
        {
            return Task.Run(async() => await action());
        }



        [TestMethod]
        public async Task TestRpcFromClientToServer()
        {
            int result = 0;
            var t1 = Run(async () =>
            {
                _server.Register("test", (arg) =>
                {
                    return new object[] { int.Parse(arg[0].ToString()) + 1 };
                });
                await _server.ConnectAsync(5000);
                _server.Start();
            });
            var t2 = Run(async () =>
            {
                await _client.ConnectAsync(5000);
                _client.Start();
                result = int.Parse((await _client.CallAsync("test",  5 ))[0].ToString());

            });
            await Task.WhenAll(t1, t2);
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public async Task TestRpcFromServerToClient()
        {
            int result = 0;
            var t1 = Run(async () =>
            {
                await _server.ConnectAsync(5000);
                _server.Start();
                result = int.Parse((await _server.CallAsync("test", 5))[0].ToString());
            });
            var t2 = Run(async () =>
            {
                await _client.ConnectAsync(5000);
                _client.Register("test", (arg) =>
                {
                    return new object[] { int.Parse(arg[0].ToString()) + 1 };
                });
                _client.Start();


            });
            await Task.WhenAll(t1, t2);
            Assert.AreEqual(6, result);
        }

        public void Dispose()
        {
            _server?.Disconnect();
            _server?.Dispose();
            _client?.Disconnect();
            _client?.Dispose();
        }
    }
}
