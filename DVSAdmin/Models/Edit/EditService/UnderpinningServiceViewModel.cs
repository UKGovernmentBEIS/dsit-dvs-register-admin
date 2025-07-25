using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class UnderpinningServiceViewModel :ServiceSummaryBaseViewModel
    {
        public List<ServiceDto> UnderpinningServices { get; set; }
        public List<ServiceDto> ManualUnderpinningServices { get; set; }
        public string? SearchText { get; set; }
        public bool IsPublished { get; set; }
    }
}
