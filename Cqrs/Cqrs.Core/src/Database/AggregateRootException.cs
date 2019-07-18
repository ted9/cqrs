using System;


namespace Cqrs.Database
{
    /// <summary>Represents an exception when tring to get a not existing aggregate root.
    /// </summary>
    [Serializable]
    public class AggregateRootException : CqrsException
    {
        private const string aggregateRootNotFound = "Cannot find the aggregate root {0} of id {1}.";


        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public AggregateRootException(Type aggregateType, object aggregateId) :
            base(string.Format(aggregateRootNotFound, aggregateType.Name, aggregateId))
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected AggregateRootException(string message)
            : base(message)
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected AggregateRootException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
