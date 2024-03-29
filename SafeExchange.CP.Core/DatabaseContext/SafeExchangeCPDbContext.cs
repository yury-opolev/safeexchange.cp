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

        public SafeExchangeCPDbContext(DbContextOptions<SafeExchangeCPDbContext> options)
            : base(options)
        { }

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

            modelBuilder.Entity<GeoInstance>()
                .HasOne(gi => gi.Location)
                .WithMany(l => l.GeoInstances)
                .HasForeignKey(gi => gi.LocationName)
                .HasPrincipalKey(l => l.Name)
                .IsRequired();

            modelBuilder.Entity<Location>()
                .ToContainer("Locations")
                .HasNoDiscriminator()
                .HasPartitionKey(l => l.PartitionKey);
        }
    }
}
