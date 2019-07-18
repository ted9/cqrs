using System.Collections.Concurrent;
using System.Threading;

namespace Cqrs.Messaging.Queue
{
    public class DefaultMessageQueueFactory : IMessageQueueFactory
    {
        class MessageQueue : IMessageQueue
        {
            private readonly ConcurrentQueue<IMessage> _queue = new ConcurrentQueue<IMessage>();
            private int _running = 1;


            public void Enqueue(IMessage message)
            {
                _queue.Enqueue(message);
            }

            public IMessage Dequeue()
            {
                IMessage message = null;
                if (!_queue.IsEmpty && Interlocked.CompareExchange(ref _running, 0, 1) == 1 && _queue.TryDequeue(out message)) {
                    return message;
                }
                return null;
            }

            public bool Ack()
            {
                return Interlocked.CompareExchange(ref _running, 1, 0) == 0;
            }
        }

        public IMessageQueue Create()
        {
            return new MessageQueue();
        }

        public IMessageQueue[] CreateGroup(int count)
        {
            Ensure.Positive(count, "count");

            IMessageQueue[] queues = new IMessageQueue[count];
            for (int i = 0; i < count; i++) {
                queues[i] = new MessageQueue();
            }

            return queues;
        }
    }
}
