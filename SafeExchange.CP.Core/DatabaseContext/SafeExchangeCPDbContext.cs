/// <summary>
/// SafeExchangeCPDbContext
/// </summary>

namespace SafeExchange.CP.Core.DatabaseContext
{
    using Microsoft.EntityFrameworkCore;
    using SafeExchange.CP.Core.Model;

    public class SafeExchangeCPDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<EntityRecord> Entities { get; set; }

        public DbSet<GeoInstance> GeoInstances { get; set; }

        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToContainer("Users")
                .HasNoDiscriminator()
                .HasPartitionKey(u => u.PartitionKey);

            modelBuilder.Entity<Application>()
                .ToContainer("Applications")
                .HasNoDiscriminator()
                .HasPartitionKey(om => om.PartitionKey);

            modelBuilder.Entity<EntityRecord>()
                .ToContainer("EntityRecords")
                .HasNoDiscriminator()
                .HasPartitionKey(om => om.PartitionKey);

            modelBuilder.Entity<GeoInstance>()
                .ToContainer("GeoInstances")
                .HasNoDiscriminator()
                .HasPartitionKey(gi => gi.PartitionKey);

            modelBuilder.Entity<GeoInstance>(
                gie =>
                {
                    gie.OwnsOne(
                        gi => gi.Location,
                        gib =>
                        {
                            gib.Property(l => l.Id).IsRequired();
                            gib.Property(l => l.DisplayName).IsRequired();
                            gib.Property(l => l.RegionalDisplayName).IsRequired();
                        });

                    gie.Navigation(gi => gi.Location).IsRequired();
                });

            modelBuilder.Entity<Location>()
                .ToContainer("Locations")
                .HasNoDiscriminator()
                .HasPartitionKey(ar => ar.PartitionKey);
        }
    }
}
