using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class CertificateInfoRoleMappingDto
    {
        public int Id { get; set; }     
        public int CertificateInformationId { get; set; }  
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
