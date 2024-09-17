﻿using DVSAdmin.CommonUtility.Models.Consent;
using DVSAdmin.CommonUtility.Models.PublicInterestCheck;

namespace DVSAdmin.CommonUtility.Models
{
    public class GovUkNotifyConfiguration
    {
        public const string ConfigSection = "GovUkNotify";
        public string ApiKey { get; set; }
        public string OfDiaEmailId { get; set; }
        public string LoginLink { get; set; }
        public string PreRegLink { get; set; }

        public PassPIPrimaryCheckTemplate PassPIPrimaryCheckTemplate { get;set; }
        public FailPIPrimaryCheckTemplate FailPIPrimaryCheckTemplate { get; set; }
        public PrimaryCheckRoundTwoTemplate PrimaryCheckRoundTwoTemplate { get; set;}
        public ApplicationRejectedTemplate ApplicationRejectedTemplate { get; set; }
        public IssueURNConfirmationTemplate IssueURNConfirmationTemplate { get; set; }
        public AccountCreatedTemplate AccountCreatedTemplate { get; set; }
        public FailedLoginAttemptTemplate FailedLoginAttemptTemplate { get; set; } 
        public ApplicationRejectedConfirmationTemplate ApplicationRejectedConfirmationTemplate { get; set; }
        public ApplicationApprovedConfirmationTemplate ApplicationApprovedConfirmationTemplate { get; set; }
        public CertificateInfoApprovedToCabTemplate CertificateInfoApprovedToCabTemplate { get; set; }
        public CertificateInfoApprovedToDSIT CertificateInfoApprovedToDSIT { get; set; }
        public CertificateInfoRejectedToCabTemplate CertificateInfoRejectedToCabTemplate { get; set; }
        public CertificateInfoRejectedToDSITTemplate CertificateInfoRejectedToDSITTemplate { get; set; }
        public DIPConsentToPublishTemplate DIPConsentToPublishTemplate { get; set; }
        public AdditionalContactConsentTemplate AdditionalContactConsentTemplate { get; set; }
        public AgreementToPublishTemplate AgreementToPublishTemplate { get; set; }
        public AgreementToPublishToDSITTemplate AgreementToPublishToDSITTemplate { get; set; }
        public ServicePublishedDIPTemplate ServicePublishedDIPTemplate { get; set; }
        public ServicePublishedCABTemplate ServicePublishedCABTemplate { get; set; }
        public ServicePublishedDSITTemplate ServicePublishedDSITTemplate { get;set; }
        public ProceedApplicationConsentToDIPTemplate ProceedApplicationConsentToDIPTemplate { get; set; }

    }
}
