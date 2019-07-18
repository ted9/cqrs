using System;
using System.Collections.Generic;
using System.Linq;

using Cqrs.Components;
using Cqrs.Database;
using Cqrs.Messaging;
using Cqrs.Infrastructure.Utilities;
using System.Reflection;


namespace Cqrs.EventSourcing
{
    /// <summary>
    /// 实现 <see cref="IEventSourced"/> 的抽象类
    /// </summary>
    [Serializable]
    public abstract class EventSourced<TIdentify> : AggregateRoot<TIdentify>, IEventSourced
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected EventSourced(TIdentify id)
            : base(id)
        {
            (this as IEventSourced).SubscribeEvents();
        }

        public int Version { get; private set; }

        
        /// <summary>
        /// Configures a handler for an event. 
        /// </summary>
        protected void Handles<TEvent>(Action<TEvent> handler)
            where TEvent : IVersionedEvent
        {
            this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected virtual void SubscribeEvents()
        {
            var entries = from method in this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                          let returnType = method.ReturnType
                          let parameters = method.GetParameters()
                          where returnType == typeof(void) && parameters.Length == 1 && typeof(IVersionedEvent).IsAssignableFrom(parameters.Single().ParameterType)
                          select new { Method = method, EventType = parameters.Single().ParameterType };

            foreach (var entry in entries) {
                this.handlers.Add(entry.EventType, @event => entry.Method.Invoke(this, new[] { @event }));
            }
        }

        [NonSerialized]
        private Dictionary<Type, Action<IVersionedEvent>> handlers;
        void IEventSourced.SubscribeEvents()
        {
            if (handlers == null) {
                handlers = new Dictionary<Type, Action<IVersionedEvent>>();
            }

            this.SubscribeEvents();
        }

        /// <summary>
        /// 收集事件。
        /// </summary>
        new protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : VersionedEvent<TIdentify>
        {
            Ensure.NotNull(@event, "@event");

            @event.Version = this.Version + 1;

            base.RaiseEvent(@event);
            this.ApplyEvent(@event, typeof(TEvent));

            this.Version = @event.Version;
        }


        void IEventSourced.LoadFrom(IEnumerable<IVersionedEvent> events)
        {
            foreach (var @event in events) {
                ApplyEvent(@event, @event.GetType());
                this.Version = @event.Version;
            }
        }

        //[NonSerialized]
        //private static IEventSourcedHandlerProvider _eventHandlerProvider = ObjectContainer.Instance.Resolve<IEventSourcedHandlerProvider>();
        private void ApplyEvent(IVersionedEvent @event, Type eventType)
        {
            if (@event.Version == 1 && this.Id.Equals(default(TIdentify))) {
                this.Id = TypeConvert.To<TIdentify>(@event.SourceId);
            }

            if (@event.Version > 1 && this.Id.ToString() != @event.SourceId)
                throw new EventSourcedException(@event.SourceId, this.Id.ToString());

            if (@event.Version != this.Version + 1)
                throw new EventSourcedException(@event.Version, this.Version);

            //this.handlers[@event.GetType()].Invoke(@event);
            //var aggregateType = this.GetType();
            var handler = this.handlers[eventType];
            if (handler == null) {
                throw new EventSourcedException(this.GetType(), eventType);
            }
            handler( @event);
        }


        IEnumerable<IVersionedEvent> IEventSourced.GetEvents()
        {
            return base.GetUnpublishedEvents().OfType<IVersionedEvent>();
        }
        
        //void IEventSourced.ClearEvents()
        //{
        //    base.ClearEvents();
        //}

        
    }
}
