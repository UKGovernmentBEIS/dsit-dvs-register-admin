using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models.Draft
{
    public class ServiceRoleMappingDraftDto
    {
        public int Id { get; set; }
        public int ServiceDraftId { get; set; }
        public ServiceDraft ServiceDraft { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}