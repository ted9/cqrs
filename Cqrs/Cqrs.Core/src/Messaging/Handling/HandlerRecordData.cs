using System;
using System.Linq;


namespace Cqrs.Messaging.Handling
{
    /// <summary>
    /// 处理程序信息
    /// </summary>
    [Serializable]
    public class HandlerRecordData : IEquatable<HandlerRecordData>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected HandlerRecordData()
        {
            this.Timestamp = DateTime.UtcNow;
        }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public HandlerRecordData(string messageId, int messageTypeCode, int handlerTypeCode)
            : this()
        {
            this.MessageId = messageId;
            this.HandlerTypeCode = handlerTypeCode;
            this.MessageTypeCode = messageTypeCode;
        }

        /// <summary>
        /// 相关id
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// 消息类型编码
        /// </summary>
        public int MessageTypeCode { get; set; }
        /// <summary>
        /// 处理器类型编码
        /// </summary>
        public int HandlerTypeCode { get; set; }

        /// <summary>
        /// 创建时间
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
        /// 确定此实例是否与指定的对象（也必须是 <see cref="HandlerRecordData"/> 对象）相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is HandlerRecordData) {
                return Equals((HandlerRecordData)obj);
            }

            return false;
        }

        /// <summary>
        /// 将此实例的标识转换为其等效的字符串表示形式。
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}", MessageId, MessageTypeCode, HandlerTypeCode);
        }

        private bool Equals(HandlerRecordData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return other.MessageId == this.MessageId
                && other.HandlerTypeCode == this.HandlerTypeCode
                && other.MessageTypeCode == this.MessageTypeCode;
        }

        bool IEquatable<HandlerRecordData>.Equals(HandlerRecordData other)
        {
            return this.Equals(other);
        }
    }
}
