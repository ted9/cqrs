using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Cqrs.Components;
using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Scheduling;
using Cqrs.Infrastructure.Serialization;
using Cqrs.Messaging.Queue;


namespace Cqrs.Messaging.Runtime
{
    public class MessageProcessor<TMessage> : DisposableObject, IMessageProcessor<TMessage>, IProcessor, IInitializer 
        where TMessage : class, IMessage
    {
        private readonly object _lockObject = new object();

        private readonly IMessageQueue[] _queues;
        private readonly int _queuesCount;
        private readonly IMessageExecutor<TMessage> _messageExecutor;

        private readonly IMessageStore _messageStore;
        private readonly ITextSerializer _serializer;

        private readonly string _messageTypeName;

        private readonly TaskScheduler _taskScheduler;

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public MessageProcessor(string messageTypeName, IMessageQueue[] queues, IMessageExecutor<TMessage> messageExecutor,
            IMessageStore messageStore, ITextSerializer serializer)
        {
            this._messageTypeName = messageTypeName;

            this._messageExecutor = messageExecutor;
            this._queuesCount = queues.Length;
            this._queues = queues;

            this._messageStore = messageStore;
            this._serializer = serializer;

            this._taskScheduler = new LimitedConcurrencyLevelTaskScheduler(_queuesCount);
        }

        /// <summary>
        /// 消息处理器名称
        /// </summary>
        public string Name 
        { 
            get { return string.Concat(_messageTypeName, "Processor"); } 
        }


        private CancellationTokenSource _cancellationSource;
        /// <summary>
        /// 启动。
        /// </summary>
        public void Start()
        {
            lock (this._lockObject) {
                if (this._cancellationSource == null) {
                    this._cancellationSource = new CancellationTokenSource();
                    Task.Factory.StartNew(
                        () => this.HandleQueueMessage(_cancellationSource.Token),
                        this._cancellationSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Current);
                }
            }
        }
        /// <summary>
        /// 停止。
        /// </summary>
        public void Stop()
        {
            lock (this._lockObject) {
                if (this._cancellationSource != null) {
                    using (this._cancellationSource) {
                        this._cancellationSource.Cancel();
                    }

                    this._cancellationSource = null;
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.Stop();
        }


        private void HandleQueueMessage(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {                
                var queueIndex = Interlocked.Increment(ref _index) % _queuesCount;

                var queue = _queues[queueIndex];
                var message = queue.Dequeue() as TMessage;
                if (message != null) {
                    _empty = 0;
                    Task.Factory.StartNew(() => {
                        Process(message);
                        queue.Ack();
                    }, cancellationToken, TaskCreationOptions.PreferFairness, _taskScheduler);
                }
                else {
                    _empty++;
                }

                if (_empty % _queuesCount == 0)
                    Thread.Sleep(100);
            }
        }

        private int _index = -1;
        private int _empty = 0;
        /// <summary>
        /// 接收消息。
        /// </summary>
        public void Receive(IEnumerable<TMessage> messages)
        {
            var datas = messages.Select(message => new MessageData {
                MessageId = message.Id,
                MessageType = _messageTypeName,
                Body = _serializer.Serialize(message),
                DeliveryDate = message.Timestamp
            });
            _messageStore.Add(datas);

            messages.ForEach(AddToQueue);
        }

        private void AddToQueue(TMessage message)
        {
            int queueIndex;
            if (message is IRouter) {
                queueIndex = ((IRouter)message).GetRoutingKey().GetHashCode() % _queuesCount;
            }
            else {
                queueIndex = message.Id.GetHashCode() % _queuesCount;
            }

            //var queueIndex = message.GetKey().GetHashCode() % _queuesCount;
            if (queueIndex < 0) {
                queueIndex = Math.Abs(queueIndex);
            }
            _queues[queueIndex].Enqueue(message);
        }

        ///// <summary>
        ///// 调用消息处理结果。
        ///// </summary>
        //protected abstract void OnMessageHandled(TMessage payload);
        /// <summary>
        /// 调用消息处理结果。
        /// </summary>
        protected virtual void Process(TMessage message)
        {
            int count = 0;
            int retryTimes = 1;
            if (message is IRetry) {
                retryTimes = (message as IRetry).RetryTimes;
            }
            while (++count <= retryTimes) {
                try {
                    _messageExecutor.Execute(message);
                    _messageStore.Remove(_messageTypeName, message.Id);
                    break;
                }
                catch (Exception) {
                    if (count == retryTimes)
                        throw;
                    else
                        Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(IContainer container, IEnumerable<Type> types)
        {
            var messages = _messageStore.GetAll(_messageTypeName);
            if (!messages.IsEmpty()) {
                messages.Select(message => _serializer.Deserialize(message.Body))
                    .OfType<TMessage>().ForEach(AddToQueue);
            }

            this.Start();
        }
    }
}
