﻿namespace ServiceBus.Channel.FileBroker
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
        private readonly IModuleCatalog _moduleCatalog;
        private readonly Dictionary<string, Queue<IMessageData>> _queues = new Dictionary<string, Queue<IMessageData>>();
        private readonly Config _config;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public FileChannel(IModuleCatalog moduleCatalog)
        {
            _moduleCatalog = moduleCatalog;
            _config = @"Config\FileBrokerConfig.json".FromJsonFile<Config>();
        }

        public void SetUp()
        {
            if (!Directory.Exists(_config.RootPath))
            {
                Directory.CreateDirectory(_config.RootPath);
            }
            foreach (var listener in _moduleCatalog.Listeners)
            {
                var path = listener.Key.Replace(".", @"\");
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
            _queues.Clear();
        }

        public void Publish(string topic, IMessageData data)
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
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    foreach (var listener in _moduleCatalog.Listeners)
                    {
                        var path = string.Empty + Path.Combine(_config.RootPath, listener.Key.Replace(".", @"\"));
                        var files = Directory.GetFiles(path, "*.msg");
                        foreach (var file in files)
                        {
                            var msg = file.FromJsonFile<MessageData>();
                            MessageReceived?.Invoke(this, MessageReceivedEventArgs.Create(listener.Value, msg));
                            File.Delete(file);
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
        }
    }
}