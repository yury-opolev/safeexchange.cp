namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;

    [PrimaryKey(nameof(EntityType), nameof(EntityName))]
    public class EntityRecord
    {
        public const string DefaultEntityType = "Object";

        public EntityRecord() { }

        public EntityRecord(string entityName, GeoInstance geoInstance)
            : this(DefaultEntityType, entityName, geoInstance)
        { }

        public EntityRecord(string entityType, string entityName, GeoInstance geoInstance)
        {
            this.EntityType = entityType;
            this.EntityName = entityName;
            this.PartitionKey = this.GetPartitionKey();

            this.GeoInstance = geoInstance;
        }

        public string PartitionKey { get; set; }

        [Required]
        public string EntityType { get; set; }

        [Required]
        public string EntityName { get; set; }

        public GeoInstance GeoInstance { get; set; }

        private string GetPartitionKey()
        {
            var hashString = $"{this.EntityType}{this.EntityName}".GetHashCode().ToString("0000");
            return hashString.Substring(hashString.Length - 4, 4);
        }
    }
}
