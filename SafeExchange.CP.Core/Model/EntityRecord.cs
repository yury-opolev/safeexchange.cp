namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// EntityRecord is a registry item that, for a given secret name, points to a safeexchange
    /// backend geo-specific instance.
    /// All secrets that exist in safeexchange, have a globally-unique name, therefore
    /// a secret name should always be present in the registry, associated with a geo-specific
    /// backend instance.
    /// </summary>
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
