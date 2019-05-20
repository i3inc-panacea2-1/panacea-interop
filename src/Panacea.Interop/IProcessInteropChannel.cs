using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public interface IProcessInteropChannel:IDisposable
    {
        event EventHandler Closed;

        void Start();

        void Register(string uri, Func<object[], object[]> callback);

        void Register(string uri, Func<object[], Task<object[]>> callback);

        void Subscribe(string uri, Action<object[]> callback);

        void Publish(string uri, params object[] args);

        Task PublishAsync(string uri, params object[] args);

        object[] Call(string uri, params object[] args);

        object[] Call(string uri, int milliSecondsTimeout, params object[] args);

        Task<object[]> CallAsync(string uri, params object[] args);

        void ReleaseSubscriptions();
    }
}
