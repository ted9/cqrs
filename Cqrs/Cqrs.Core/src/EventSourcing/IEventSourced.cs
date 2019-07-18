using System.Collections.Generic;
using Cqrs.Database;
using Cqrs.Messaging;

namespace Cqrs.EventSourcing
{
    public interface IEventSourced : IAggregateRoot
    {
        int Version { get; }

        void SubscribeEvents();

        IEnumerable<IVersionedEvent> GetEvents();

        void LoadFrom(IEnumerable<IVersionedEvent> events);
    }
}
