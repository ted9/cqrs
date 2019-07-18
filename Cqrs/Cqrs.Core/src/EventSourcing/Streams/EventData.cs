using System;
using System.Linq;

namespace Cqrs.EventSourcing.Streams
{
    /// <summary>
    /// 历史事件(用于还原溯源聚合的事件)
    /// </summary>
    [Serializable]
    public class EventData : IEquatable<EventData>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EventData()
        {
            this.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EventData(int aggregateRootTypeCode, string aggregateRootId, int version)
            : this()
        {
            this.AggregateRootId = aggregateRootId;
            this.AggregateRootTypeCode = aggregateRootTypeCode;
            this.Version = version;            
        }

        /// <summary>
        /// 聚合根标识。
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>
        /// 聚合根类型编码。
        /// </summary>
        public int AggregateRootTypeCode { get; set; }
        /// <summary>
        /// 版本号。
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 事件流
        /// </summary>
        public string Payload { get; set; }
        /// <summary>
        /// 发布事件的相关id
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// 生成事件的时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 返回此实例的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// 确定此实例是否与指定的对象（也必须是 <see cref="EventData"/> 对象）相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is EventData) {
                return Equals((EventData)obj);
            }

            return false;
        }

        /// <summary>
        /// 将此实例的标识转换为其等效的字符串表示形式。
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}", AggregateRootTypeCode, AggregateRootId, Version);
        }

        private bool Equals(EventData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return other.AggregateRootTypeCode == this.AggregateRootTypeCode
                && other.AggregateRootId == this.AggregateRootId 
                && other.Version == this.Version;
        }

        bool IEquatable<EventData>.Equals(EventData other)
        {
            return this.Equals(other);
        }
    }
}
