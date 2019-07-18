using System;

namespace Cqrs.Messaging
{
    /// <summary>
    /// 实现 <see cref="ICommand"/> 的抽象类
    /// </summary>
    [Serializable]
    public abstract class Command : Message, ICommand, IRetry
    {
        /// <summary>
        /// 重试次数。
        /// </summary>
        protected int RetryTimes { get; set; } 

        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected Command()
            : this(null)
        { }
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        protected Command(string id)
            : base(id)
        {
            this.RetryTimes = 5;
        }

        /// <summary>
        /// 获取聚合根标识的字符串形式
        /// </summary>
        protected virtual string AggregateRootStringId
        {
            get { return null; }
        }

        
        int IRetry.RetryTimes
        {
            get { return this.RetryTimes; }
        }
        
        string ICommand.AggregateRootId
        {
            get { return this.AggregateRootStringId; }
        }
    }

    /// <summary>
    /// Represents an abstract aggregate command.
    /// </summary>
    [Serializable]
    public abstract class Command<TAggregateRootId> : Command
    {
        /// <summary>
        /// Represents the aggregate root which is related with the command.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; private set; }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Command(TAggregateRootId aggregateRootId)
            : this(null, aggregateRootId)
        { }
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        protected Command(string commandId, TAggregateRootId aggregateRootId)
            : base(commandId)
        {
            this.AggregateRootId = aggregateRootId;
        }

        protected override string AggregateRootStringId
        {
            get { return this.AggregateRootId.ToString(); }
        }
        /// <summary>
        /// Returns the aggregate root id as the key.
        /// </summary>
        protected override string GetRoutingKey()
        {
            return this.AggregateRootStringId.Safe(base.Id);
        }
    }
}
