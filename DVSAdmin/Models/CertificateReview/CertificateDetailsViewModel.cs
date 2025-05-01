using DVSAdmin.Models.CertificateReview;

namespace DVSAdmin.Models
{
    public class CertificateDetailsViewModel   
    {       
        public CertficateRejectionViewModel? CertficateRejection { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }
        public CertificateValidationViewModel? CertificateValidation { get; set; }           
        public SendBackViewModel? SendBackViewModel { get; set; }
        public bool CanResendOpeningLoopRequest { get; set; }
    }
}
