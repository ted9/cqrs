using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Cqrs.Infrastructure.Serialization;
using Cqrs.Infrastructure.Storage;
using Cqrs.Messaging.Queue;


namespace Cqrs.Infrastructure.Stores
{
    public class DefaultMessageStore : IMessageStore
    {
        private readonly ITextSerializer _serializer;
        private readonly IDataContextFactory _dbContextFactory;
        private readonly bool _persistent;

        public DefaultMessageStore(IDataContextFactory dbContextFactory, ITextSerializer serializer)
        {
            this._dbContextFactory = dbContextFactory;
            this._serializer = serializer;
            this._persistent = ConfigurationManager.AppSettings["thinkcfg.message_persist"].Safe("false").ToBoolean();
        }


        public void Add(IEnumerable<MessageData> messages)
        {
            if (!_persistent || messages.IsEmpty()) return;

            using (var context = _dbContextFactory.CreateDataContext()) {
                messages.ForEach(context.Save);
                context.Commit();
            }
        }

        public void Remove(string messageType, string messageId)
        {
            if (!_persistent) return;

            using (var context = _dbContextFactory.CreateDataContext()) {
                //context.CreateQuery<MessageData>().Where(message =>
                //    message.MessageType == messageType &&
                //    message.MessageId == messageId
                //).ToList().ForEach(context.Delete);                
                context.Delete(new MessageData {
                    MessageType = messageType,
                    MessageId = messageId
                });
                context.Commit();
            }
        }

        public IEnumerable<MessageData> GetAll(string messageType)
        {
            if (!_persistent) 
                return Enumerable.Empty<MessageData>();

            using (var context = _dbContextFactory.CreateDataContext()) {
                return context.CreateQuery<MessageData>()
                    .Where(message => message.MessageType == messageType)
                    .OrderBy(@event => @event.DeliveryDate)
                    .ToList();
            }
        }

        public bool PersistEnabled
        {
            get { return _persistent; }
        }
    }
}
