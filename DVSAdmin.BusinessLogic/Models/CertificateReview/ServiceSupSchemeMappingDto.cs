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
    }
}
