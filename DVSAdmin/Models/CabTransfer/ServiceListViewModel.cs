using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Models.UI;

namespace DVSAdmin.Models.CabTransfer
{
    public class ServiceListViewModel : PaginationParameters
    {
        public List<ServiceDto>? Services { get; set; }
        public string? SearchText { get; set; }
    }
}
