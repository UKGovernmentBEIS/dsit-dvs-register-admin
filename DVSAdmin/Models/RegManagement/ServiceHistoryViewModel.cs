using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Models.UI;

namespace DVSAdmin.Models.RegManagement
{
    public class ServiceHistoryViewModel : PaginationParameters
    {
        public List<ServiceDto>? Services { get; set; }
        public int ServiceKey { get; set; } 
    }
}
