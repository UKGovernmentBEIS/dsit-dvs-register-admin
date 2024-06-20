using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class ProviderDetailsViewModel
    {
        public ProviderDto Provider { get; set; }
        public List<CertificateInformationDto> CertificateInformationList { get; set; }
    }
}
