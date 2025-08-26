using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Models.UI;

namespace DVSAdmin.Models.Home
{
    public class PendingListViewModel : PaginationParameters
    {
        public OpenTaskCount OpenTaskCount { get; set; }
        public  List<ServiceDto> PendingRequests { get; set; }      
    }
}
