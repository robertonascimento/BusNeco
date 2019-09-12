namespace ServiceBus.Channel.FileBroker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Infra.Entities;
    using Infra.Interfaces;

    public class FileChannel : IChannel
    {
        private readonly HandlerCatalog _moduleCatalog;
        private readonly Config _config;
       

        public FileChannel(HandlerCatalog moduleCatalog)
        {
            _moduleCatalog = moduleCatalog;
            _config = @"Config\FileBrokerConfig.json".FromJsonFile<Config>();
        }

        public string ChannelId { get; }

        public void SetUp()
        {
            if (!Directory.Exists(_config.RootPath))
            {
                Directory.CreateDirectory(_config.RootPath);
            }
            foreach (var listener in _moduleCatalog.Binders)
            {
                var path = listener.Key.Replace(".", @"\");
                path = Path.Combine(_config.RootPath, path);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            foreach (var responder in _moduleCatalog.Binders)
            {
                var path = responder.Key.Replace(".", @"\");
                path = Path.Combine(_config.RootPath, path);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            ListenQueues();
        }

        public void Close()
        {
            //
        }

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        
        public MessageData Request(string subject, MessageData data, TimeSpan timeOut) {
            throw new NotImplementedException();
        }

        public void AddBinders(IDictionary<string, MethodMetadata> binders) {
  
        }

        public void Publish(string topic, MessageData data)
        {
            var path = topic.Replace(".", @"\");
            path = Path.Combine(_config.RootPath, path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fileName = Path.Combine(path, $"{DateTime.Now.Ticks}");
            File.WriteAllText($"{fileName}.tmp", data.ToJson());
            File.Move($"{fileName}.tmp", $"{fileName}.msg");
        }

        private void ListenQueues()
        {
            WatchListeners();
            WatchResponders();
        }

        private void WatchResponders()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    foreach (var responder in _moduleCatalog.Binders)
                    {
                        var path = string.Empty + Path.Combine(_config.RootPath, responder.Key.Replace(".", @"\"));
                        var files = Directory.GetFiles(path, "*.msg");
                        foreach (var file in files)
                        {
                            var msg = file.FromJsonFile<MessageData>();
                            OnMessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(responder.Value, msg));
                            File.Delete(file);
                        }
                    }
                    Thread.Sleep(50);
                }
            });
        }

        private void WatchListeners()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    foreach (var listener in _moduleCatalog.Binders)
                    {
                        var path = string.Empty + Path.Combine(_config.RootPath, listener.Key.Replace(".", @"\"));
                        var files = Directory.GetFiles(path, "*.msg");
                        foreach (var file in files)
                        {
                            var msg = file.FromJsonFile<MessageData>();
                            OnMessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(listener.Value, msg));
                            File.Delete(file);
                        }
                    }
                    Thread.Sleep(50);
                }
            });
        }

    }
}
