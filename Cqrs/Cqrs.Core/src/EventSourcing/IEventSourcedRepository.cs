using Cqrs.Components;
using Cqrs.Infrastructure.Repositories;

namespace Cqrs.EventSourcing
{
    [RequiredComponent(typeof(DefaultEventSourcedRepository))]
    public interface IEventSourcedRepository
    {
        TAggregateRoot Get<TAggregateRoot>(object id) where TAggregateRoot : class, IEventSourced;

        TAggregateRoot Find<TAggregateRoot>(object id) where TAggregateRoot : class, IEventSourced;

        void Save(IEventSourced aggregateRoot, string correlationId = null);
        void Remove(IEventSourced aggregateRoot);
    }
}
