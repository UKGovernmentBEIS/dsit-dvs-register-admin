using DVSAdmin.BusinessLogic.Models.CAB;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class CertificateInformationDto
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
        public string ServiceName { get; set; }

        public ICollection<CertificateInfoRoleMappingDto> CertificateInfoRoleMapping { get; set; }
        public ICollection<CertificateInfoIdentityProfileMappingDto> CertificateInfoIdentityProfileMapping { get; set; }
        public bool HasSupplementarySchemes { get; set; }
        public ICollection<CertificateInfoSupSchemeMappingDto>? CertificateInfoSupSchemeMappings { get; set; }
        public string FileName { get; set; }
        public string FileLink { get; set; }
        public DateTime ConformityIssueDate { get; set; }
        public DateTime ConformityExpiryDate { get; set; }
        public CertificateInfoStatusEnum CertificateInfoStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public CertificateReviewDto CertificateReview { get; set; }

        public int DaysLeftToComplete { get; set; }
        public List<RoleDto> Roles { get; set; }
        public List<IdentityProfileDto> IdentityProfiles { get; set; }
        public List<SupplementarySchemeDto>? SupplementarySchemes { get; set; }

        public string? SubmittedCAB { get; set; }

    }
}
