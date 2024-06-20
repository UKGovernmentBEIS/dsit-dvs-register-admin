using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class ConsentViewModel
    {       
        public string? token { get; set; }       
        public CertificateInformationDto? CertificateInformation { get; set; }
    }
}
