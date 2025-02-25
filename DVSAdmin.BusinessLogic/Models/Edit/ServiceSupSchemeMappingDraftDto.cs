using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models.Draft
{
    public class ServiceSupSchemeMappingDraftDto
    {
        
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraft ServiceDraft { get; set; }
        public int SupplementarySchemeId { get; set; }
        public SupplementaryScheme SupplementaryScheme { get; set; }
    }
}