using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class CertificateReviewListViewModel
    {
        public List<ServiceDto>? CertificateReviewList { get; set; }

        public List<ServiceDto>? ArchiveList { get; set; }
    }
}
