using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models.Enums;
using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class CertificateReviewDto
    {
        public int Id { get; set; }     
        public int ServiceId { get; set; }

        [JsonIgnore]
        public ServiceDto Service { get; set; }
        public int ProviProviderProfileId { get; set; }
        public ProviderProfileDto ProviderProfile { get; set; }     
      
        public string? Amendments { get; set; }
        public bool? CertificateValid { get; set; }
        public bool? InformationMatched { get; set; }
  
        public string? RejectionComments { get; set; }      
        public int VerifiedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public CertificateReviewEnum CertificateReviewStatus { get; set; }
        public List<CertificateReviewRejectionReasonMappingDto>? CertificateReviewRejectionReasonMapping { get; set; }
    }
}
