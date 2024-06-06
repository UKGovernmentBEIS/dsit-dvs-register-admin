using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Entities
{
    public class CertificateInformation :BaseEntity
    {
        public CertificateInformation() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("PreRegistration")]
        public int PreRegistrationId { get; set; }
        public PreRegistration PreRegistration { get; set; }
        public string RegisteredName { get; set; }
        public string TradingName { get; set; }
        public string PublicContactEmail { get; set; }
        public string TelephoneNumber { get; set; }
        public string WebsiteAddress { get; set; }
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public ICollection<CertificateInfoRoleMapping> CertificateInfoRoleMapping { get; set; }
        public ICollection<CertificateInfoIdentityProfileMapping> CertificateInfoIdentityProfileMapping { get; set; }
        public bool HasSupplementarySchemes { get; set; }
        public ICollection<CertificateInfoSupSchemeMapping>? CertificateInfoSupSchemeMappings { get; set; }

        public string FileName { get; set; }
        public string FileLink { get; set; }
        public DateTime ConformityIssueDate { get; set; }
        public DateTime ConformityExpiryDate { get; set; }
        public CertificateInfoStatusEnum CertificateInfoStatus { get;set; }
        public CertificateReview CertificateReview { get; set; }
        public string? SubmittedCAB { get; set; }
    }
}
