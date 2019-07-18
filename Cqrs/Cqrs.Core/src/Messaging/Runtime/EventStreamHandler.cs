using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cqrs.Messaging.Runtime
{
    internal class EventStreamHandler : IHandler<EventStream>
    {
        private readonly IEventExecutor _eventExecutor;
        private readonly IEventBus _eventBus;
        private readonly IEventPublishedVersionStore _eventPublishedVersionStore;
        private readonly ConcurrentQueue<EventStream> _eventQueue;

        public EventStreamHandler(IEventExecutor eventExecutor, IEventBus eventBus,
            IEventPublishedVersionStore eventPublishedVersionStore)
        {
            this._eventExecutor = eventExecutor;
            this._eventBus = eventBus;
            this._eventPublishedVersionStore = eventPublishedVersionStore;
            this._eventQueue = new ConcurrentQueue<EventStream>();
        }

        private void AdjacentEventExecutedAndVersionPublishedUntilSuccessed()
        {
            EventStream @event;
            while (!_eventQueue.IsEmpty) {
                if (!_eventQueue.TryDequeue(out @event)) {
                    break;
                }

                if (AdjacentEventExecutedAndVersionPublished(@event))
                    break;
            }
        }

        private bool AdjacentEventExecutedAndVersionPublished(EventStream @event)
        {
            var version = _eventPublishedVersionStore.GetPublishedVersion(@event.AggregateRootTypeCode, @event.AggregateRootId);
            //var version = _eventPublishedVersionStore.GetPublishedVersion(new EventPublishedVersionData {
            //    AggregateRootId = @event.AggregateRootId,
            //    AggregateRootTypeCode = @event.AggregateRootTypeCode
            //});

            if (version++ != @event.StartVersion) {
                _eventQueue.Enqueue(@event);
                return false;
            }

            EventExecutedAndVersionPublished(@event);
            return true;
        }

        private void EventExecutedAndVersionPublished(EventStream @event)
        {
            @event.Events.ForEach(TryExecuteEvent);

            var versionData = new EventPublishedVersionData {
                AggregateRootId = @event.AggregateRootId,
                AggregateRootTypeCode = @event.AggregateRootTypeCode,
                Version = @event.EndVersion
            };
            if (@event.StartVersion == 1) {
                _eventPublishedVersionStore.WriteFirstVersion(versionData);
            }
            else {
                _eventPublishedVersionStore.UpdatePublishedVersion(versionData);
            }
        }

        public void Handle(EventStream eventStream)
        {
            if (eventStream.Events.IsEmpty())
                return;

            if (eventStream.StartVersion == 1) {
                EventExecutedAndVersionPublished(eventStream);
                return;
            }

            if (AdjacentEventExecutedAndVersionPublished(eventStream))
                return;

            AdjacentEventExecutedAndVersionPublishedUntilSuccessed();
        }

        private void TryExecuteEvent(IEvent @event)
        {
            int count = 0;
            int retryTimes = 5;
            if (@event is IRetry) {
                retryTimes = (@event as IRetry).RetryTimes;
            }
            while (++count <= retryTimes) {
                try {
                    _eventExecutor.Execute(@event);
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
    }
}
