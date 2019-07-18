using System;
using System.Collections.Generic;

using Cqrs.Components;


namespace Cqrs.Messaging.Handling
{
    [RequiredComponent(typeof(DefaultHandlerProvider))]
    public interface IHandlerProvider
    {
        /// <summary>
        /// 获取该消息类型的所有的处理程序。
        /// </summary>
        IEnumerable<IProxyHandler> GetHandlers(Type type);
    }
}
