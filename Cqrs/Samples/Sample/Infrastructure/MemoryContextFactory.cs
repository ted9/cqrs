using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Cqrs.Components;
using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Storage;


namespace CqrsSample.Infrastructure
{
    [RegisteredComponent(typeof(IDataContextFactory))]
    public class MemoryContextFactory : IDataContextFactory
    {
        class MemoryContext : DataContextBase
        {
            private static Dictionary<Type, IList<object>> entityCollection = new Dictionary<Type, IList<object>>();

            public override IDbConnection DbConnection
            {
                get { throw new NotImplementedException(); }
            }

            protected override void Dispose(bool disposing)
            { }

            public override System.Collections.ICollection TrackingObjects
            {
                get { return localNewCollection.Union(localModifiedCollection).Union(localDeletedCollection).ToArray(); }
            }

            private readonly List<object> localNewCollection = new List<object>();
            private readonly List<object> localModifiedCollection = new List<object>();
            private readonly List<object> localDeletedCollection = new List<object>();
            protected override void DoCommit()
            {
                foreach (var newObj in localNewCollection) {
                    Add(newObj);
                }
                foreach (var modifiedObj in localModifiedCollection) {
                    Remove(modifiedObj);
                    Add(modifiedObj);
                }
                foreach (var delObj in localDeletedCollection) {
                    Remove(delObj);
                }

                localNewCollection.Clear();
                localModifiedCollection.Clear();
                localDeletedCollection.Clear();
            }

            private void Add(object entity)
            {
                var entityType = entity.GetType();

                IList<object> entities;

                if (!entityCollection.TryGetValue(entityType, out entities)) {
                    entities = new List<object>();
                    entityCollection.Add(entityType, entities);
                }

                if (!entities.Contains(entity)) {
                    entities.Add(entity);
                    entityCollection[entityType] = entities;
                }
            }
            private void Remove(object entity)
            {
                var entityType = entity.GetType();

                IList<object> entities;

                if (entityCollection.TryGetValue(entityType, out entities)) {
                    var index = entities.IndexOf(entity);

                    if (index != -1) {
                        entities.RemoveAt(index);
                        entityCollection[entityType] = entities;
                    }
                }
            }

            public override bool Contains<TEntity>(TEntity entity)
            {
                return localNewCollection.Any(item => entity.Equals(item))
                     || localModifiedCollection.Any(item => entity.Equals(item))
                     || entityCollection[entity.GetType()].Any(item => entity.Equals(item));
            }

            public override void Detach<TEntity>(TEntity entity)
            {
                if (localDeletedCollection.Contains(entity)) {
                    localDeletedCollection.Remove(entity);
                }

                if (localModifiedCollection.Contains(entity)) {
                    localModifiedCollection.Remove(entity);
                }

                if (localNewCollection.Contains(entity)) {
                    localNewCollection.Remove(entity);
                }
            }

            public override object Get(Type type, params object[] keyValues)
            {
                Activator.CreateInstance(type, keyValues);

                Func<object, bool> expression = entity => {
                    return Activator.CreateInstance(type, keyValues).Equals(entity);
                };

                if (localNewCollection.Any(expression)) {
                    return localNewCollection.FirstOrDefault(expression);
                }

                if (localModifiedCollection.Any(expression)) {
                    return localNewCollection.FirstOrDefault(expression);
                }

                IList<object> entities;
                if (entityCollection.TryGetValue(type, out entities)) {
                    return entities.First(expression);
                }

                return null;
            }

            public override void Refresh<TEntity>(TEntity entity)
            { }

            public override void Save<TEntity>(TEntity entity)
            {
                if (localDeletedCollection.Contains(entity))
                    throw new Exception("The object cannot be registered as a new object since it was marked as deleted.");

                if (localModifiedCollection.Contains(entity))
                    throw new Exception("The object cannot be registered as a new object since it was marked as modified.");

                //if (localNewCollection.Contains(entity))
                //    throw new AggregateException("The object cannot be registered as a new object, because the same id already exist.");
                if (localNewCollection.Contains(entity))
                    localNewCollection.Remove(entity);

                localNewCollection.Add(entity);
            }

            public override void Update<TEntity>(TEntity entity)
            {
                if (localDeletedCollection.Contains(entity))
                    throw new Exception("The object cannot be registered as a modified object since it was marked as deleted.");

                if (localNewCollection.Contains(entity))
                    throw new Exception("The object cannot be registered as a modified object since it was marked as created.");

                if (localModifiedCollection.Contains(entity))
                    localModifiedCollection.Remove(entity);

                localModifiedCollection.Add(entity);
            }

            public override void Delete<TEntity>(TEntity entity)
            {
                if (localNewCollection.Contains(entity)) {
                    if (localNewCollection.Remove(entity))
                        return;
                }
                bool removedFromModified = localModifiedCollection.Remove(entity);
                if (!localDeletedCollection.Contains(entity)) {
                    localDeletedCollection.Add(entity);
                }
            }


            public override IQueryable<TEntity> CreateQuery<TEntity>()
            {
                IList<object> entities;
                if (entityCollection.TryGetValue(typeof(TEntity), out entities)) {
                    IEnumerable<TEntity> data = entities.Cast<TEntity>();

                    return new EnumerableQuery<TEntity>(data);
                }

                return new EnumerableQuery<TEntity>(new TEntity[0]);
            }
        }

        public IDataContext GetCurrentDataContext()
        {
            throw new NotImplementedException();
        }

        public IDataContext CreateDataContext()
        {
            return new MemoryContext();
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
