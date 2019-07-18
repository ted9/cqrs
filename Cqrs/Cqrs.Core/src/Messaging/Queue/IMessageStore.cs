using System.Collections.Generic;
using Cqrs.Components;
using Cqrs.Infrastructure.Stores;

namespace Cqrs.Messaging.Queue
{
    [RequiredComponent(typeof(DefaultMessageStore))]
    public interface IMessageStore
    {
        /// <summary>
        /// 是否启用持久化存储。
        /// </summary>
        bool PersistEnabled { get; }

        /// <summary>
        /// Persist a new message to the queue.
        /// </summary>
        void Add(IEnumerable<MessageData> messages);
        /// <summary>
        /// Remove a existing message from the queue.
        /// </summary>
        void Remove(string messageType, string messageId);
        /// <summary>
        /// Get all the existing messages of the queue.
        /// </summary>
        IEnumerable<MessageData> GetAll(string messageType);
        ///// <summary>
        ///// Get all the existing messages of the queue.
        ///// </summary>
        //IEnumerable<IMessage> GetAll(string messageType, int rows);
    }
}
