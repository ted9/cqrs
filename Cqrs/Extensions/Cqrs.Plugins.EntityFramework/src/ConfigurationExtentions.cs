using System;
using System.Data.Entity;

using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Storage;
using Cqrs.Plugins.EntityFramework;


namespace Cqrs.Configurations
{
    public static class ConfigurationExtentions
    {

        public static Configuration RegisterDbContextFactory(this Configuration that, Func<DbContext> contextFactory, string contextType = null)
        {
            if (string.IsNullOrWhiteSpace(contextType)) {
                that.RegisterInstance<IDataContextFactory>(new EntityFrameworkContextFactory(contextFactory));
            }
            else {
                that.RegisterInstance<IDataContextFactory>(new EntityFrameworkContextFactory(contextFactory, contextType));
            }
            return that;
        }
    }
}
