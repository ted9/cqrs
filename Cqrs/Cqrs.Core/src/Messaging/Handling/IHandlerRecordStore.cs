using Cqrs.Components;
using Cqrs.Infrastructure.Stores;

namespace Cqrs.Messaging.Handling
{
    /// <summary>
    /// 存储处理程序信息的接口
    /// </summary>
    [RequiredComponent(typeof(DefaultHandlerRecordStore))]
    public interface IHandlerRecordStore
    {
        /// <summary>
        /// 添加处理程序信息
        /// </summary>
        void AddHandlerInfo(HandlerRecordData handlerInfo);
        /// <summary>
        /// 检查该处理程序信息是否存在
        /// </summary>
        bool IsHandlerInfoExist(HandlerRecordData handlerInfo);
    }
}
