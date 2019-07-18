using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Messaging.Runtime
{
    [Serializable]
    internal class EventStream : Message, IEvent
    {
        public EventStream()
            : base(null)
        { }

        /// <summary>
        /// 聚合根标识。
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>
        /// 聚合根类型编码。
        /// </summary>
        public int AggregateRootTypeCode { get; set; }
        /// <summary>
        /// 产生事件的命令标识
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// 起始版本号
        /// </summary>
        public int StartVersion { get; set; }
        /// <summary>
        /// 结束版本号
        /// </summary>
        public int EndVersion { get; set; }
        /// <summary>
        /// 事件源
        /// </summary>
        public IEnumerable<IEvent> Events { get; set; }

        /// <summary>
        /// 获取路由值
        /// </summary>
        protected override string GetRoutingKey()
        {
            return this.AggregateRootId.Safe(CommandId);
        }

        string IEvent.SourceId
        {
            get { return this.AggregateRootId; }
        }

        public override string ToString()
        {
            return string.Format("[EventId={0},CommandId={1},AggregateRootId={2},AggregateRootTypeCode={3},Version={4}~{5},Events={6}]",
                Id,
                CommandId,
                AggregateRootId,
                AggregateRootTypeCode,
                StartVersion,
                EndVersion,
                string.Join("|", Events.Select(item => item.GetType().Name)));
        }
    }
}
