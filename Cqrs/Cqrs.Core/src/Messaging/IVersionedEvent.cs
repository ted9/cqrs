﻿using System;


namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示继承该接口的类型是一个有序事件。
    /// </summary>
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// 版本号
        /// </summary>
        int Version { get; }
    }
}
