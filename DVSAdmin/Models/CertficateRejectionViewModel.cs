using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Validations;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertficateRejectionViewModel
    {     
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }          

        [Required(ErrorMessage = "Required")]
        public string? Comments { get; set; }

        public List<CertificateReviewRejectionReasonDto>? RejectionReasons { get; set; }

        [EnsureMinimumCount(ErrorMessage = "Select the reasons")]
        public List<int>? SelectedRejectionReasonIds { get; set; }
        public List<CertificateReviewRejectionReasonDto>? SelectedReasons { get; set; }

        public string ? CabUserEmail { get; set; }
    }
}
