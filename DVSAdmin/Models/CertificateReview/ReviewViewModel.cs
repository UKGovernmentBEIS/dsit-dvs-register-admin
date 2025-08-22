using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models.CertificateReview
{
    public class ReviewViewModel
    {
        public int ServiceId { get; set; }
        public ServiceDto? Service { get; set; }

        [Required(ErrorMessage = "Select whether the certificate is valid")]
        public bool? CertificateValid { get; set; }

        [Required(ErrorMessage = "Select whether the information matches")]
        public bool? InformationMatched { get; set; }

        public bool? IsSendBack { get; set; }

        public string? ReviewComments { get; set; }

    }
}
