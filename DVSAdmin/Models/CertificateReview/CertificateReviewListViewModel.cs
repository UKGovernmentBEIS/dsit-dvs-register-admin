using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class CertificateReviewListViewModel
    {
        public List<ServiceDto>? CertificateReviewList { get; set; }

        public List<ServiceDto>? ArchiveList { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string CurrentSort { get; set; } = "days";
        public string CurrentSortAction { get; set; } = "ascending";
        public string? SearchText { get; set; }
    }
}
