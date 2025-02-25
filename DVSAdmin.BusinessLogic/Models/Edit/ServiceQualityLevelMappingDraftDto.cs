using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models.Draft
{
    public class ServiceQualityLevelMappingDraftDto
    {
        
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraft ServiceDraft { get; set; }
        public int QualityLevelId { get; set; }
        public QualityLevel QualityLevel { get; set; }
    }
}