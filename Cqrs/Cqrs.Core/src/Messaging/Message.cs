using System;
using Cqrs.Infrastructure.Utilities;

namespace Cqrs.Messaging
{
    /// <summary>
    /// <see cref="IMessage"/> 的抽象实现类
    /// </summary>
    [Serializable]
    public abstract class Message : IMessage, IRouter
    {
        /// <summary>
        /// 消息标识
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// 消息的时间戳
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        protected Message(string id)
        {
            this.Id = id.Safe(GuidUtil.NewSequentialId().ToString());
            this.Timestamp = DateTime.UtcNow;
        }
        /// <summary>
        /// 输出消息的字符串格式
        /// </summary>
        public override string ToString()
        {
            return ObjectUtils.GetObjectString(this);
        }

        /// <summary>
        /// 获取消息的路由值。
        /// </summary>
        protected virtual string GetRoutingKey()
        {
            return this.Id;
        }


        string IRouter.GetRoutingKey()
        {
            return this.GetRoutingKey();
        }
    }
}
