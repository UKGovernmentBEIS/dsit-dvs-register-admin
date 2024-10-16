namespace DVSAdmin.Models
{
    public class CertificateDetailsViewModel   
    {
        public string? OpeningTheloopLink { get; set; }
        public CertficateRejectionViewModel? CertficateRejection { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }
        public CertificateValidationViewModel? CertificateValidation { get; set; }       
    }
}
