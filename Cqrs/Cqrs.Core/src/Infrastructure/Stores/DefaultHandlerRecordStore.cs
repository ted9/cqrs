using System;
using System.Linq;

using Cqrs.Messaging.Handling;
using Cqrs.Infrastructure.Storage;


namespace Cqrs.Infrastructure.Stores
{
    public class DefaultHandlerRecordStore : HandlerRecordInMemory
    {
        private readonly IDataContextFactory _contextFactory;
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public DefaultHandlerRecordStore(IDataContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }
        /// <summary>
        /// 添加处理程序信息
        /// </summary>
        public override void AddHandlerInfo(HandlerRecordData handlerInfo)
        {
            base.AddHandlerInfo(handlerInfo);

            using (var context = _contextFactory.CreateDataContext()) {
                if (IsHandlerInfoExist(context, handlerInfo))
                    return;

                context.Save(handlerInfo);
                context.Commit();
            }
        }

        private static bool IsHandlerInfoExist(IDataContext context, HandlerRecordData handleInfo)
        {
            try {
                return context.CreateQuery<HandlerRecordData>()
                    .Any(p => p.MessageId == handleInfo.MessageId &&
                        p.MessageTypeCode == handleInfo.MessageTypeCode &&
                        p.HandlerTypeCode == handleInfo.HandlerTypeCode);
            }
            catch (Exception) {                
                throw;
            }            
        }

        /// <summary>
        /// 检查该处理程序信息是否存在
        /// </summary>
        public override bool IsHandlerInfoExist(HandlerRecordData handlerInfo)
        {
            if (base.IsHandlerInfoExist(handlerInfo))
                return true;

            using (var context = _contextFactory.CreateDataContext()) {
                return IsHandlerInfoExist(context, handlerInfo);
            }
        }
    }
}
