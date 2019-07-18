using System;
using Cqrs.Database;

namespace Cqrs.EventSourcing
{
    /// <summary>Represents an event-sourced exception.
    /// </summary>
    [Serializable]
    public class EventSourcedException : AggregateRootException
    {
        private const string differentAggregateRoot = "Cannot apply event to aggregate root as the AggregateRootId not matched. DomainEvent SourceId:{0}; Current AggregateRootId:{1}";
        private const string differentAggregateRootVersion = "Cannot apply event to aggregate root as the version not matched. DomainEvent Version:{0}; Current AggregateRoot Version:{1}";
        private const string innerEventHandlerNotFound = "Event handler not found on {0} for {1}.";


        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EventSourcedException(Type aggregateType, object aggregateId) :
            base(aggregateType, aggregateId)
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EventSourcedException(string eventSourceId, string aggregateRootId) :
            base(string.Format(differentAggregateRoot, eventSourceId, aggregateRootId)) 
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EventSourcedException(int eventSourceVersion, int aggregateRootVersion) :
            base(string.Format(differentAggregateRootVersion,eventSourceVersion, aggregateRootVersion))
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EventSourcedException(Type aggregateType, Type eventType) :
            base(string.Format(innerEventHandlerNotFound, aggregateType.FullName, eventType.FullName))
        { }
    }
}
