namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;

    [PrimaryKey(nameof(Id))]
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(InstanceBaseUrl), IsUnique = true)]
    public class GeoInstance
    {
        public const string DefaultPartitionKey = "GEO";

        public GeoInstance() { }

        public GeoInstance(string displayName, string instancePrefix, string instanceBaseUrl, Location location, bool isDefault, string createdBy)
        {
            this.Id = Guid.NewGuid().ToString();
            this.PartitionKey = DefaultPartitionKey;

            this.DisplayName = displayName;
            this.InstancePrefix = instancePrefix;
            this.InstanceBaseUrl = instanceBaseUrl;

            this.Location = location;
            this.IsDefault = isDefault;

            this.CreatedAt = DateTimeProvider.UtcNow;
            this.CreatedBy = createdBy;
        }

        public string Id { get; set; }

        public string PartitionKey { get; set; }

        public string LocationName { get; set; }

        public Location Location { get; set; }

        public bool IsDefault { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Value too long (150 character limit).")]
        [RegularExpression(@"^[a-zA-Z0-9-]+( [a-zA-Z0-9-]+)*$", ErrorMessage = "Only letters, numbers, hyphens and spaces are allowed, starting with non-space symbol.")]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Value too long (3 character limit).")]
        [RegularExpression(@"^[a-zA-Z0-9]{3}$", ErrorMessage = "Only letters and numbers are allowed.")]
        public string InstancePrefix { get; set; }

        [Required]
        public string InstanceBaseUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
