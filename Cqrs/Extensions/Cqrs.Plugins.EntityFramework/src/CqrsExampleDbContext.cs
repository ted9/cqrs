using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using Cqrs.EventSourcing.Streams;
using Cqrs.Messaging.Handling;

namespace Cqrs.Infrastructure.Storage
{
    public class CqrsDbContext : DbContext
    {
        public CqrsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = true;
            this.Configuration.LazyLoadingEnabled = true;
        }


        private EntityTypeConfiguration<EventData> EventDataConfiguration()
        {
            var config = new EntityTypeConfiguration<EventData>();

            config.HasKey(@event => new { @event.AggregateRootId, @event.AggregateRootTypeCode, @event.Version });
            config.Property(@event => @event.AggregateRootId).IsRequired().HasColumnType("char").HasMaxLength(36);
            config.Property(@event => @event.AggregateRootTypeCode).IsRequired().HasColumnType("int");
            config.Property(@event => @event.Version).HasColumnType("int");
            config.Property(@event => @event.CorrelationId).HasColumnType("char").HasMaxLength(36);
            config.Property(@event => @event.Payload).HasColumnType("varchar");
            config.Property(@event => @event.Timestamp).HasColumnName("OnCreated").HasColumnType("datetime");

            config.ToTable("Cqrs_events");

            return config;
        }

        private EntityTypeConfiguration<HandlerRecordData> HandlerInfoConfiguration()
        {
            var config = new EntityTypeConfiguration<HandlerRecordData>();

            config.HasKey(handler => new { handler.MessageId, handler.MessageTypeCode, handler.HandlerTypeCode });
            config.Property(handler => handler.MessageId).IsRequired().HasColumnType("char").HasMaxLength(36);
            config.Property(handler => handler.MessageTypeCode).IsRequired().HasColumnType("int");
            config.Property(handler => handler.HandlerTypeCode).HasColumnType("int");
            config.Property(handler => handler.Timestamp).HasColumnName("OnCreated").HasColumnType("datetime");

            config.ToTable("Cqrs_handlers");

            return config;
        }

        private EntityTypeConfiguration<SnapshotData> SnapshotConfiguration()
        {
            var config = new EntityTypeConfiguration<SnapshotData>();

            config.HasKey(snapshot => new { snapshot.AggregateRootId, snapshot.AggregateRootTypeCode });
            config.Property(snapshot => snapshot.AggregateRootId).IsRequired().HasColumnType("char").HasMaxLength(36);
            config.Property(snapshot => snapshot.AggregateRootTypeCode).IsRequired().HasColumnType("int");
            config.Property(snapshot => snapshot.Version).HasColumnType("int");
            config.Property(snapshot => snapshot.Data).HasColumnType("varchar");
            config.Property(snapshot => snapshot.Timestamp).HasColumnName("OnCreated").HasColumnType("datetime");

            config.ToTable("Cqrs_snapshots");

            return config;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations
                .Add(EventDataConfiguration())
                .Add(HandlerInfoConfiguration())
                .Add(SnapshotConfiguration());
        }
    }
}