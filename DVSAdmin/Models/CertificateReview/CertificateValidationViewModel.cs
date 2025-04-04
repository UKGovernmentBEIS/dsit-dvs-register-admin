using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertificateValidationViewModel
    {     

        public int ServiceId { get; set; }
        public ServiceDto? Service { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid logos and certificate number")]
        public bool? IsCabLogoCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid Conformity Assessment Body details")]
        public bool? IsCabDetailsCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid service provider details")]
        public bool? IsProviderDetailsCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid name of service")]
        public bool? IsServiceNameCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid roles certified against")]
        public bool? IsRolesCertifiedCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid scope of certification")]
        public bool? IsCertificationScopeCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid summary of service")]
        public bool? IsServiceSummaryCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid URL link to service")]
        public bool? IsURLLinkToServiceCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid GPG44 quality assessment")]
        public bool? IsGPG44Correct { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid GPG45 identity profiles")]
        public bool? IsGPG45Correct { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid methods of service provision")]
        public bool? IsServiceProvisionCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid geographical location")]
        public bool? IsLocationCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid date of issue")]
        public bool? IsDateOfIssueCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains a valid date of expiry")]
        public bool? IsDateOfExpiryCorrect { get; set; }

        [Required(ErrorMessage = "Select correct if the certificate contains valid authenticity verification")]
        public bool? IsAuthenticyVerifiedCorrect { get; set; }

        [Required(ErrorMessage = "Enter the reasons for your decision")]
        public string? CommentsForIncorrect { get; set; }

    }
}
