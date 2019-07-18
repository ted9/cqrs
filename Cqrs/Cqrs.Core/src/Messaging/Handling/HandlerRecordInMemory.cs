using System;
using System.Collections.Generic;
using Cqrs.Infrastructure.Scheduling;


namespace Cqrs.Messaging.Handling
{
    /// <summary>
    /// 将已完成的处理程序信息记录在内存中。
    /// </summary>
    public class HandlerRecordInMemory : IHandlerRecordStore
    {
        private readonly HashSet<HandlerInfoWrapper> _handlerInfoSet;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public HandlerRecordInMemory()
        {
            this._handlerInfoSet = new HashSet<HandlerInfoWrapper>();

            Dispatcher.Create("HandleInfoTask", RemoveHandleInfo).SetInterval(60000 * 20).Start();
        }

        /// <summary>
        /// 移除超出期限的信息
        /// </summary>
        protected virtual void RemoveHandleInfo()
        {
            _handlerInfoSet.RemoveWhere(item => item.TimeStamp.AddHours(1) < DateTime.Now);
        }

        /// <summary>
        /// 添加处理程序信息
        /// </summary>
        public virtual void AddHandlerInfo(HandlerRecordData handlerInfo)
        {
            _handlerInfoSet.Add(new HandlerInfoWrapper(handlerInfo));
        }
        

        /// <summary>
        /// 检查该处理程序信息是否存在
        /// </summary>
        public virtual bool IsHandlerInfoExist(HandlerRecordData handlerInfo)
        {
            return _handlerInfoSet.Contains(new HandlerInfoWrapper(handlerInfo));
        }

        class HandlerInfoWrapper : HandlerRecordData
        {
            public HandlerInfoWrapper(HandlerRecordData handleInfo)
            {
                this.MessageId = handleInfo.MessageId;
                this.HandlerTypeCode = handleInfo.HandlerTypeCode;
                this.MessageTypeCode = handleInfo.MessageTypeCode;
                this.TimeStamp = DateTime.Now;
            }

            public DateTime TimeStamp { get; private set; }
        }
    }
}
