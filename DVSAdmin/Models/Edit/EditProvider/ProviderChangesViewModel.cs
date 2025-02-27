using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class ProviderChangesViewModel
    {
        public ProviderProfileDto? CurrentProvider { get; set; }
        public ProviderProfileDraftDto? ChangedProvider { get; set; }

        public string DSITUserEmails { get; set; }
    }
}
