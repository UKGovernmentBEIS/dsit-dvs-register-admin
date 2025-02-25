using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models.Draft
{
    public class ServiceIdentityProfileMappingDraftDto
    {
        
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraft ServiceDraft { get; set; }
        public int IdentityProfileId { get; set; }
        public IdentityProfile IdentityProfile { get; set; }
    }
}