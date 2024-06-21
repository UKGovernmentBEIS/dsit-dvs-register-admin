using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models.RegManagement
{
    public class ProviderListViewModel
    {
        public List<ProviderDto> ActionRequiredList { get; set; }
        public List<ProviderDto> PublicationCompleteList { get; set; }
    }
}
