using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models.CertificateReview
{
    public class SendBackViewModel
    {
        [Required(ErrorMessage = "Enter the amendments needed for the CAB")]
        public string? Reason { get; set; }
        public string? CommentFromReview { get; set; }
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }
    }
}


