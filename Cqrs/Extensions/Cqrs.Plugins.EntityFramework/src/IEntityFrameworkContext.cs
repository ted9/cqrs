using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Storage;


namespace Cqrs.Plugins.EntityFramework
{
    public interface IEntityFrameworkContext : IDataContext
    {
        DbContext DbContext { get; }

        IQueryable<TEntity> CreateQuery<TEntity>(params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties) where TEntity : class;
        
        IEnumerable<TEntity> SqlQuery<TEntity>(string sql, params object[] parameters) where TEntity : class;

        int SqlExecute(string sql, params object[] parameters);
    }

}
