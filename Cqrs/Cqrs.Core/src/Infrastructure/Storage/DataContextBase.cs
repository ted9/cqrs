﻿using System;
using System.Collections;
using System.Data;
using System.Linq;

using Cqrs.Contexts;


namespace Cqrs.Infrastructure.Storage
{
    /// <summary>
    /// 实现 <see cref="IDbContext"/> 的抽象类
    /// </summary>
    public abstract class DataContextBase : DisposableObject, IDataContext, IContext
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DataContextBase()
        { }
        private readonly IContextManager _contextManager;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected DataContextBase(IContextManager contextManager)
        {
            this._contextManager = contextManager;
        }

        /// <summary>
        /// 当前的数据连接
        /// </summary>
        public abstract IDbConnection DbConnection { get; }

        private void Validate(object entity)
        {
            if (entity is IValidatable) {
                (entity as IValidatable).Validate(this);
            }
        }
        private LifecycleVeto Callback(object entity, Func<ILifecycle, IDataContext, LifecycleVeto> action)
        {
            if (entity is ILifecycle) {
                return action(entity as ILifecycle, this);
            }
            return LifecycleVeto.Accept;
        }
        private static LifecycleVeto OnSaving(ILifecycle entity, IDataContext context)
        {
            return (entity as ILifecycle).OnSaving(context);
        }
        private static LifecycleVeto OnUpdating(ILifecycle entity, IDataContext context)
        {
            return (entity as ILifecycle).OnUpdating(context);
        }
        private static LifecycleVeto OnDeleting(ILifecycle entity, IDataContext context)
        {
            return (entity as ILifecycle).OnDeleting(context);
        }
       

        /// <summary>
        /// 预处理事务。
        /// </summary>
        protected abstract void DoCommit();


        /// <summary>
        /// 获取跟踪的对象集合
        /// </summary>
        public abstract ICollection TrackingObjects { get; }


        /// <summary>
        /// 提交事务。
        /// </summary>
        public void Commit()
        {
            this.DoCommit();

            this.DataCommitted(this, EventArgs.Empty);
        }

        /// <summary>
        /// 新增一个新对象到当前上下文
        /// </summary>
        public abstract void Save<TEntity>(TEntity entity) where TEntity : class;
        void IDataContext.Save<TEntity>(TEntity entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnSaving) == LifecycleVeto.Veto)
                return;

            this.Save(entity);
        }

        /// <summary>
        /// 修改一个对象到当前上下文
        /// </summary>
        public abstract void Update<TEntity>(TEntity entity) where TEntity : class;
        void IDataContext.Update<TEntity>(TEntity entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnUpdating) == LifecycleVeto.Veto)
                return;

            this.Update(entity);
        }
        
        /// <summary>
        /// 删除一个对象到当前上下文
        /// </summary>
        public abstract void Delete<TEntity>(TEntity entity) where TEntity : class;
        void IDataContext.Delete<TEntity>(TEntity entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnDeleting) == LifecycleVeto.Veto)
                return;

            this.Delete(entity);
        }

        /// <summary>
        /// 当前工作单元是否包含此实体
        /// </summary>
        public abstract bool Contains<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 从当前工作分离此实体
        /// </summary>
        public abstract void Detach<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 从数据库刷新最新状态的实体
        /// </summary>
        public abstract void Refresh<TEntity>(TEntity entity) where TEntity : class;
        void IDataContext.Refresh<TEntity>(TEntity entity)
        {
            this.Refresh(entity);

            if (entity != null && entity is ILifecycle) {
                (entity as ILifecycle).OnLoaded(this);
            }
        }
        /// <summary>
        /// 获取实体信息
        /// </summary>
        public abstract object Get(Type type, params object[] keyValues);
        object IDataContext.Get(Type type, params object[] keyValues)
        {
            var entity = this.Get(type, keyValues);
            if (entity != null && entity is ILifecycle) {
                (entity as ILifecycle).OnLoaded(this);
            }

            return entity;
        }
        /// <summary>
        /// 获取实体信息
        /// </summary>
        public virtual TEntity Get<TEntity>(params object[] keyValues) 
            where TEntity : class
        {
            return (TEntity)this.Get(typeof(TEntity), keyValues);
        }
        TEntity IDataContext.Get<TEntity>(params object[] keyValues)
        {
            var entity = this.Get<TEntity>(keyValues);
            if (entity != null && entity is ILifecycle) {
                (entity as ILifecycle).OnLoaded(this);
            }

            return entity;
        }

        /// <summary>
        /// 获取对数据类型已知的特定数据源的查询进行计算的功能。
        /// </summary>
        public abstract IQueryable<TEntity> CreateQuery<TEntity>() where TEntity : class;
        

        /// <summary>
        /// 在数据提交成功后执行
        /// </summary>
        public event EventHandler DataCommitted = (sender, args) => { };

        IContextManager IContext.ContextManager
        {
            get { return this._contextManager; }
        }        

    }
}
