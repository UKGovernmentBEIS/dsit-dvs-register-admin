using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class CertificateReviewListViewModel
    {
        public List<CertificateInformationDto>? CertificateReviewList { get; set; }

        public List<CertificateInformationDto>? ArchiveList { get; set; }
    }
}
