//namespace ServiceBus.Channel.RabbitMq
//{
//    using System.Linq;
//    using System.Collections.Generic;
//    using System.Threading.Tasks;
//    using System;
//    using Infra.Entities;
//    using Infra.Interfaces;

//    public class MemChannel : IChannel
//    {
//        private readonly IModuleCatalog _moduleCatalog;
//        private readonly Dictionary<string, Queue<IMessageData>> _queues = new Dictionary<string, Queue<IMessageData>>();
//        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

//        public MemChannel(IModuleCatalog moduleCatalog)
//{
//    _moduleCatalog = moduleCatalog;
//}


//        public void SetUp()
//        {
//            ListenQueues();
//        }

//        public void Close()
//        {
//            _queues.Clear();
//        }

//        public void Publish(string topic, IMessageData data)
//        {
//            if (!_queues.ContainsKey(topic))
//            {
//                _queues.Add(topic, new Queue<IMessageData>());
//            }
//            _queues[topic].Enqueue(data);
//        }

//        private void ListenQueues()
//        {
//            Task.Factory.StartNew(() =>
//            {
//                while (true)
//                    foreach (var queue in _queues.Where(q => q.Value.Count != 0))
//                    {
//                        var topic = queue.Key;
//                        if (!_handlerCatalog.Listeners.ContainsKey(topic)) continue;
//                        var listener = _handlerCatalog.Listeners[topic];
//                        MessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(listener, queue.Value.Dequeue()));
//                    }
//            });
//        }
//    }
//}
