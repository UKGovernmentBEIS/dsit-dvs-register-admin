using DVSAdmin.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceRoleMappingDto
    {
        [Key]
        public int Id { get; set; }     
        public int ServiceId { get; set; }

        [JsonIgnore]
        public ServiceDto Service { get; set; }    
        public int RoleId { get; set; }
        public RoleDto Role { get; set; }
    }
}
