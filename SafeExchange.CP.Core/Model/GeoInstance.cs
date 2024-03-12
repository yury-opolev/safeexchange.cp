namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using SafeExchange.CP.Core.Utilities;
    using System.ComponentModel.DataAnnotations;

    [PrimaryKey(nameof(Id))]
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(InstanceBaseUrl), IsUnique = true)]
    public class GeoInstance
    {
        public GeoInstance() { }

        public GeoInstance(string displayName, string instanceBaseUrl, Location location, string createdBy)
        {
            this.Id = Guid.NewGuid().ToString();
            this.DisplayName = displayName;
            this.InstanceBaseUrl = instanceBaseUrl;

            this.Location = location;

            this.CreatedAt = DateTimeProvider.UtcNow;
            this.CreatedBy = createdBy;
        }

        public string Id { get; set; }

        public Location Location { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Value too long (150 character limit).")]
        [RegularExpression(@"^[a-zA-Z0-9-]+( [a-zA-Z0-9-]+)*$", ErrorMessage = "Only letters, numbers, hyphens and spaces are allowed, starting with non-space symbol.")]
        public string DisplayName { get; set; }

        [Required]
        public string InstanceBaseUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
