using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models.RegManagement
{
    public class ProviderListViewModel
    {
        public List<ProviderProfileDto> ActionRequiredList { get; set; }
        public List<ProviderProfileDto> PublicationCompleteList { get; set; }
    }
}
