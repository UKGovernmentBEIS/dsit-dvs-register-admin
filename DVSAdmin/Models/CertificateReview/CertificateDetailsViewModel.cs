using DVSAdmin.Models.CertificateReview;

namespace DVSAdmin.Models
{
    public class CertificateDetailsViewModel   
    {
        public ReviewViewModel? CertificateReview { get; set; }
        public RejectionViewModel? CertficateRejection { get; set; }      
                 
        public SendBackViewModel? SendBackViewModel { get; set; }
        public bool CanResendOpeningLoopRequest { get; set; }
    }
}
