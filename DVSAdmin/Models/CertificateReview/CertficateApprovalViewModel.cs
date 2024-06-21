using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class CertificateApprovalViewModel
    {
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }

        public string? Email {  get; set; }
        public PreRegistrationDto? PreRegistration { get; set; }
    }
}
