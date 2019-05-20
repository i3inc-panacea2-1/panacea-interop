using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panacea.Interop
{
    public class ProcessInteropChannel: IProcessInteropChannel
    {
        Random rnd = new Random();
        Dictionary<int, TaskCompletionSource<object[]>> _tasks = new Dictionary<int, TaskCompletionSource<object[]>>();
        static object _lock = new object();
        Thread _listener;
        protected Stream stream;
        bool _stopped = false;
        StreamWriter _writer;
        Dictionary<string, Func<object[], object[]>> _registrations = new Dictionary<string, Func<object[], object[]>>();
        Dictionary<string, Func<object[], Task<object[]>>> _registrationsAsync = new Dictionary<string, Func<object[], Task<object[]>>>();
        Dictionary<string, Action<object[]>> _subscriptions = new Dictionary<string, Action<object[]>>();
        public event EventHandler Closed;

        protected ProcessInteropChannel()
        {

        }

        public ProcessInteropChannel(Stream stream)
        {
            this.stream = stream;
        }

        private int CreateTask()
        {
            var code = 0;
            lock (_lock)
            {
                do
                {
                    code = rnd.Next(int.MaxValue);
                } while (_tasks.Keys.Contains(code));
                var source = new TaskCompletionSource<object[]>();
                _tasks.Add(code, source);
            }
            return code;
        }

        public virtual void Start()
        {
            if (stream == null) return;
            _writer = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            _listener = new Thread(Listen)
            {
                IsBackground = true
            };
            _listener.Start();
        }

        bool _closed = false;

        private void Listen()
        {
            var reader = new StreamReader(stream);
            _closed = false;
            try
            {
                while (!_stopped)
                {
                    var text = reader.ReadLine();
                    if (text == null) continue;
                    OnMessageReceived(JsonSerializer.DeserializeFromString<Message>(text.Trim('\0').Replace("\u200B", "")));
                }
            }
            catch
            {
                OnClose();
            }
        }

        protected void OnMessageReceived(Message msg)
        {
            Task.Run(async() =>
            {
                try
                {
                    switch (msg.Type)
                    {
                        case MessageType.Call:
                            object[] result=null;
                            if (_registrations.ContainsKey(msg.Uri))
                            {
                                result = _registrations[msg.Uri](msg.Args);
                            }else if (_registrationsAsync.ContainsKey(msg.Uri))
                            {
                                result = await _registrationsAsync[msg.Uri](msg.Args);
                            }
                            Write(MessageType.Result, msg.Id, msg.Uri, result);
                            break;
                        case MessageType.Result:
                            if (!_tasks.ContainsKey(msg.Id)) return;
                            var task = _tasks[msg.Id];
                            _tasks.Remove(msg.Id);
                            task.SetResult(msg.Args);
                            break;
                        case MessageType.Event:
                            if (_subscriptions.ContainsKey(msg.Uri))
                            {
                                _subscriptions[msg.Uri](msg.Args);
                            }
                            break;
                    }
                }
                catch (TaskCanceledException)
                {

                }
            });
        }

        public void Register(string uri, Func<object[], object[]> callback)
        {
            _registrations.Add(uri, callback);
        }

        public void Register(string uri, Func<object[], Task<object[]>> callback)
        {
            _registrationsAsync.Add(uri, callback);
        }

        public void Subscribe(string uri, Action<object[]> callback)
        {
            _subscriptions.Add(uri, callback);
        }

        public void Publish(string uri, params object[] args)
        {
            Write(MessageType.Event, 0, uri, args);
        }
        public Task PublishAsync(string uri, params object[] args)
        {
            return WriteAsync(MessageType.Event, 0, uri, args);
        }

        public object[] Call(string uri, params object[] args)
        {
            return Call(uri, -1, args);
        }
        public object[] Call(string uri, int milliSecondsTimeout, params object[] args)
        {
            var id = CreateTask();
            var task = _tasks[id].Task;
            Write(MessageType.Call, id, uri, args);
            return AsyncHelpers.RunSync(() => task, milliSecondsTimeout);
        }

        public async Task<object[]> CallAsync(string uri, params object[] args)
        {
            var id = CreateTask();
            var task = _tasks[id].Task;
            await WriteAsync(MessageType.Call, id, uri, args);
            return await task;
        }

        private void Write(int type, int id, string uri, params object[] args)
        {
            try
            {
                var str = JsonSerializer.SerializeToString(new Message() { Type = type, Id = id, Uri = uri, Args = args });
               
                _writer.WriteLine(str);
            }
            catch
            {
                OnClose();
            }
        }

        private async Task WriteAsync(int type, int id, string uri, params object[] args)
        {
            try
            {
                var str = JsonSerializer.SerializeToString(new Message() { Type = type, Id = id, Uri = uri, Args = args });
                
                await _writer.WriteLineAsync(str);
            }
            catch
            {
                OnClose();
            }
        }

        object _closedLock = new object();
        
        public void ReleaseSubscriptions()
        {
            _subscriptions.Clear();
        }

        protected virtual void OnClose()
        {
            lock (_closedLock)
            {
                
                var keys = _tasks.Keys;
                foreach (var task in keys)
                {
                    _tasks[task].SetResult(null);
                }
                _tasks.Clear();
                if (_closed) return;
                Closed?.Invoke(this, EventArgs.Empty);
                _closed = true;
            }
        }

        public virtual void Dispose()
        {
            stream?.Close();
            stream?.Dispose();
        }
    }

    internal static class MessageType
    {
        public const int Call = 0;
        public const int Result = 1;
        public const int Event = 3;
    }

    public class Message
    {
        public int Type { get; set; }

        public int Id { get; set; }

        public string Uri { get; set; }

        public object[] Args { get; set; }
    }
}
