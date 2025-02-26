

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceIdentityProfileMappingDraftDto
    {
        
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraftDto ServiceDraft { get; set; }
        public int IdentityProfileId { get; set; }
        public IdentityProfileDto IdentityProfile { get; set; }
    }
}