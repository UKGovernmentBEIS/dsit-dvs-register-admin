using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models.CabTransfer
{
    public class ServiceListViewModel
    {
        public List<ServiceDto>? Services { get; set; }
        public string? SearchText { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentSortAction { get; set; }
    }
}
