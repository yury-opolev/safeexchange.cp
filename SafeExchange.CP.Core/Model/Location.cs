namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using SafeExchange.CP.Core.Model.Dto.Output;

    [PrimaryKey(nameof(Name))]
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(RegionalDisplayName), IsUnique = true)]
    public class Location
    {
        public const string DefaultPartitionKey = "LOCATION";

        public Location() { }

        public Location(string name, string displayName, string regionalDisplayName)
        {
            this.Name = name;
            this.PartitionKey = DefaultPartitionKey;

            this.DisplayName = displayName;
            this.RegionalDisplayName = regionalDisplayName;
        }

        public string Name { get; set; }

        public string PartitionKey { get; set; }

        public string DisplayName { get; set; }

        public string RegionalDisplayName { get; set; }

        internal LocationOutput ToDto() => new()
        {
            Name = this.Name,
            DisplayName = this.DisplayName,
            RegionalDisplayName = this.RegionalDisplayName,
        };
    }
}
