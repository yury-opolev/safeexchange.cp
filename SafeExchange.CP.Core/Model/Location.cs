namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;

    [PrimaryKey(nameof(Id))]
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(RegionalDisplayName), IsUnique = true)]
    public class Location
    {
        public const string DefaultPartitionKey = "LOCATION";

        public Location(string displayName, string regionalDisplayName, bool isDefault)
        {
            this.Id = Guid.NewGuid().ToString();
            this.PartitionKey = DefaultPartitionKey;

            this.DisplayName = displayName;
            this.RegionalDisplayName = regionalDisplayName;
        }

        public string Id { get; set; }

        public string PartitionKey { get; set; }

        public string DisplayName { get; set; }

        public string RegionalDisplayName { get; set; }
    }
}
