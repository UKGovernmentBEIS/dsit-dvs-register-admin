using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class ServiceChangesViewModel
    {
        public ServiceDto? CurrentService { get; set; }
        public ServiceDraftDto? ChangedService { get; set; }
        public string DSITUserEmails { get; set; }
    }
}
