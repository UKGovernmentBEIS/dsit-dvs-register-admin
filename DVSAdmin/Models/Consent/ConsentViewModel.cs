using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Validations;

namespace DVSAdmin.Models
{
    public class ConsentViewModel
    {
        [MustBeTrue(ErrorMessage = "Select ‘I agree to publish these details on the register’")]        
        public bool? HasConsent { get; set; }
        public string? token { get; set; }       
        public CertificateInformationDto? CertificateInformation { get; set; }
    }
}
