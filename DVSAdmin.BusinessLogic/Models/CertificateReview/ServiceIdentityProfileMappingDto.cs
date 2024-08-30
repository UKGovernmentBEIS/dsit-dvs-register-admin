using DVSAdmin.Data.Entities;
using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceIdentityProfileMappingDto
    {
        public int Id { get; set; }
    
        public int ServiceId { get; set; }

        [JsonIgnore]
        public ServiceDto Service { get; set; }      
        public int IdentityProfileId { get; set; }
        public IdentityProfileDto IdentityProfile { get; set; }
    }
}
