﻿using System;
using System.Linq;

namespace Cqrs.EventSourcing.Streams
{
    /// <summary>
    /// 聚合快照
    /// </summary>
    [Serializable]
    public class SnapshotData : IEquatable<SnapshotData>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SnapshotData()
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public SnapshotData(int aggregateRootTypeCode, string aggregateRootId)
        {
            this.Timestamp = DateTime.UtcNow;
            this.AggregateRootId = aggregateRootId;
            this.AggregateRootTypeCode = aggregateRootTypeCode;
        }

        /// <summary>
        /// 聚合根标识
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>
        /// 聚合根类型名称
        /// </summary>
        public int AggregateRootTypeCode { get; set; }
        /// <summary>
        /// 创建该聚合快照的聚合根版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 聚合根数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 创建该快照的时间
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
        /// 确定此实例是否与指定的对象（也必须是 <see cref="SnapshotData"/> 对象）相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is SnapshotData) {
                return Equals((SnapshotData)obj);
            }

            return false;
        }

        /// <summary>
        /// 将此实例的标识转换为其等效的字符串表示形式。
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}_{1}", AggregateRootTypeCode, AggregateRootId);
        }

        private bool Equals(SnapshotData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return other.AggregateRootTypeCode == this.AggregateRootTypeCode
                && other.AggregateRootId == this.AggregateRootId;
        }

        bool IEquatable<SnapshotData>.Equals(SnapshotData other)
        {
            return this.Equals(other);
        }
    }
}
