using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.Draft;

namespace DVSAdmin.Models.Edit.EditService
{
    public class ServiceChangesViewModel
    {
        public ServiceDto? CurrentService { get; set; }
        public ServiceDraftDto? ChangedService { get; set; }
    }
}
