using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceIdentityProfileMappingDto
    {
        public int Id { get; set; }
    
        public int ServiceId { get; set; }
        public Service Service { get; set; }      
        public int IdentityProfileId { get; set; }
        public IdentityProfile IdentityProfile { get; set; }
    }
}
