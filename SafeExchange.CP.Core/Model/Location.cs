namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using SafeExchange.CP.Core.Model.Dto.Output;
    using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string Name { get; set; }

        public string PartitionKey { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string RegionalDisplayName { get; set; }

        public ICollection<GeoInstance> GeoInstances { get; set; }

        internal LocationOutput ToDto() => new()
        {
            Name = this.Name,
            DisplayName = this.DisplayName,
            RegionalDisplayName = this.RegionalDisplayName,
        };
    }
}
