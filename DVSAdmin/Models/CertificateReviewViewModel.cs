using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertificateReviewViewModel
    {
        public int CertificateReviewId { get; set; }     
        public CertificateInformationDto? CertificateInformation { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Comments { get; set; }

        [Required (ErrorMessage =  "Select an option")]
        public bool? InformationMatched { get; set; }
        public string? SubmitValidation { get; set; }
    }
}
