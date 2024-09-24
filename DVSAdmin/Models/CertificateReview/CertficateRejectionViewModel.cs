using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Validations;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertficateRejectionViewModel
    {     
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public CertificateReviewViewModel? CertificateReview { get; set; }          

        [Required(ErrorMessage = "Give further details on reasons selected and any required amendments")]
        public string? Comments { get; set; }

        public List<CertificateReviewRejectionReasonDto>? RejectionReasons { get; set; }

        [EnsureMinimumCount(ErrorMessage = "Select one or more reasons for rejection")]
        public List<int>? SelectedRejectionReasonIds { get; set; }
        public List<CertificateReviewRejectionReasonDto>? SelectedReasons { get; set; }

      
    }
}
