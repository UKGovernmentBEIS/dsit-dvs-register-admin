﻿using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Models
{
    public class CertificateReviewDto
    {
        public int Id { get; set; }
        public int PreRegistrationId { get; set; }       
        public int CertificateInformationId { get; set; }   

        public bool IsCabLogoCorrect { get; set; }
        public bool IsCabDetailsCorrect { get; set; }
        public bool IsProviderDetailsCorrect { get; set; }
        public bool IsServiceNameCorrect { get; set; }
        public bool IsRolesCertifiedCorrect { get; set; }
        public bool IsCertificationScopeCorrect { get; set; }
        public bool IsServiceSummaryCorrect { get; set; }
        public bool IsURLLinkToServiceCorrect { get; set; }
        public bool IsIdentityProfilesCorrect { get; set; }
        public bool IsQualityAssessmentCorrect { get; set; }
        public bool IsServiceProvisionCorrect { get; set; }
        public bool IsLocationCorrect { get; set; }
        public bool IsDateOfIssueCorrect { get; set; }
        public bool IsDateOfExpiryCorrect { get; set; }
        public bool IsAuthenticyVerifiedCorrect { get; set; }
        public string CommentsForIncorrect { get; set; }
        public bool? InformationMatched { get; set; }
        public string? Comments { get; set; }
       
        public int VerifiedUser { get; set; }
        public CertificateInfoStatusEnum CertificateInfoStatus { get; set; }
        public ICollection<CertificateReviewRejectionReasonMappingsDto>? CertificateReviewRejectionReasonMappings { get; set; }
        public string? RejectionComments { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
