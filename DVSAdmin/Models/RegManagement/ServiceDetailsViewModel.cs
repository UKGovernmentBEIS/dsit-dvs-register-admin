using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class ServiceDetailsViewModel
    {       
        public int ServiceNumber { get; set; }
        public CertificateInformationDto CertificateInformation { get; set; }
    }
}
