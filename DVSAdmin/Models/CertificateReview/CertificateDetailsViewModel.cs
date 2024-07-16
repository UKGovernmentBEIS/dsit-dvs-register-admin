using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class CertificateDetailsViewModel   
    {
        public CertficateRejectionViewModel? CertficateRejection { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public PreRegistrationDto? PreRegistration { get; set; }
    }
}
