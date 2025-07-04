using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceSupSchemeMappingDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }

        [JsonIgnore]
        public ServiceDto Service { get; set; }
        public int SupplementarySchemeId { get; set; }
        public SupplementarySchemeDto SupplementaryScheme { get; set; }
        public bool? HasGpg44Mapping { get; set; }
        [JsonIgnore]
        public ICollection<SchemeGPG44MappingDto>? SchemeGPG44Mapping { get; set; }
        [JsonIgnore]
        public ICollection<SchemeGPG45MappingDto>? SchemeGPG45Mapping { get; set; }

    }
}
