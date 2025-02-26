using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceQualityLevelMappingDraftDto
    {
        
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraftDto ServiceDraft { get; set; }
        public int QualityLevelId { get; set; }
        public QualityLevelDto QualityLevel { get; set; }
    }
}