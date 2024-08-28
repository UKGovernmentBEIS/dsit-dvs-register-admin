using DVSAdmin.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceRoleMappingDto
    {
        [Key]
        public int Id { get; set; }     
        public int ServiceId { get; set; }
        public Service Service { get; set; }    
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
