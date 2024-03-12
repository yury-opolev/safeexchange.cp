namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;

    [PrimaryKey(nameof(Id))]
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(RegionalDisplayName), IsUnique = true)]
    public class Location
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string RegionalDisplayName { get; set; }
    }
}
