using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertificateValidationViewModel
    {
        public int PreRegistrationId { get; set; }
        public int CertificateInformationId { get; set; }
        public CertificateInformationDto? CertificateInformation { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsCabLogoCorrect { get; set; }

        [Required (ErrorMessage = "Select an option")]
        public bool? IsCabDetailsCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsProviderDetailsCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsServiceNameCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsRolesCertifiedCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsCertificationScopeCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsServiceSummaryCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsURLLinkToServiceCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsIdentityProfilesCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsQualityAssessmentCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsServiceProvisionCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsLocationCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsDateOfIssueCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsDateOfExpiryCorrect { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsAuthenticyVerifiedCorrect { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? CommentsForIncorrect { get; set; }
        
    }
}
