using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Models
{
    public class IdentityProfileDto
    {
        public int Id { get; set; }
        public string IdentityProfileName { get; set; }
        public IdentityProfileTypeEnum IdentityProfileType { get; set; }
    }
}
