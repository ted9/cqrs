using System;
using System.Data.Entity;
using Cqrs.Contexts;
using Cqrs.Infrastructure.Storage;

namespace Cqrs.Plugins.EntityFramework
{
    public class EntityFrameworkContextFactory : ContextManager, IDataContextFactory
    {
        private readonly Func<DbContext> _contextFactory;
        public EntityFrameworkContextFactory(Func<DbContext> contextFactory, string contextType)
            : base(contextType)
        {
            this._contextFactory = contextFactory;
        }

        public EntityFrameworkContextFactory(Func<DbContext> contextFactory)
            : this(contextFactory, null)
        { }


        public IDataContext GetCurrentDataContext()
        {
            return base.CurrentContext.CurrentContext() as IDataContext;
        }

        public IDataContext CreateDataContext()
        {
            return new EntityFrameworkContext(_contextFactory.Invoke(), this);
        }


        public IDataContext CreateDataContext(string nameOrConnectionString)
        {
            throw new NotImplementedException();
        }


        public IDataContext CreateDataContext(System.Data.Common.DbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
