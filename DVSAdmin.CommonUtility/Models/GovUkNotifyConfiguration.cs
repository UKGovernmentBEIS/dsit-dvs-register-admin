﻿using DVSAdmin.CommonUtility.Models.EmailTemplates.Edit;
using DVSAdmin.CommonUtility.Models.PublicInterestCheck;

namespace DVSAdmin.CommonUtility.Models
{
    public class GovUkNotifyConfiguration
    {
        public const string ConfigSection = "GovUkNotify";
        public string ApiKey { get; set; }
        public string OfDiaEmailId { get; set; }
        public string LoginLink { get; set; }
      

        public PassPIPrimaryCheckTemplate PassPIPrimaryCheckTemplate { get;set; }
        public FailPIPrimaryCheckTemplate FailPIPrimaryCheckTemplate { get; set; }
        public PICheckRoundTwoTemplate PICheckRoundTwoTemplate { get; set;}
        public PICheckApplicationRejectedDIPTemplate PICheckApplicationRejectedDIPTemplate { get; set; }
        public PICheckApplicationRejectedDISTTemplate PICheckApplicationRejectedDISTTemplate { get; set; }
        public PICheckApplicationApprovedDISTTemplate PICheckApplicationApprovedDISTTemplate { get; set; }

      
        public AccountCreatedTemplate AccountCreatedTemplate { get; set; }
        public FailedLoginAttemptTemplate FailedLoginAttemptTemplate { get; set; } 
     
      
        public CertificateInfoApprovedToCabTemplate CertificateInfoApprovedToCabTemplate { get; set; }
        public CertificateInfoApprovedToDSIT CertificateInfoApprovedToDSIT { get; set; }
        public CertificateInfoRejectedToCabTemplate CertificateInfoRejectedToCabTemplate { get; set; }
        public CertificateInfoRejectedToDSITTemplate CertificateInfoRejectedToDSITTemplate { get; set; }
        public CertificateReviewRestoredToDSITTemplate CertificateReviewRestoredToDSITTemplate { get; set; }
        public CertificateSentBackToCabTemplate CertificateSentBackToCabTemplate { get; set; }
        public CertificateSentBackDSITTemplate CertificateSentBackDSITTemplate { get; set; }
        public DIPConsentToPublishTemplate DIPConsentToPublishTemplate { get; set; }      
     
        public ServicePublishedDIPTemplate ServicePublishedDIPTemplate { get; set; }
        public ServicePublishedCABTemplate ServicePublishedCABTemplate { get; set; }
        public ServicePublishedDSITTemplate ServicePublishedDSITTemplate { get;set; }
        public ProceedApplicationConsentToDIPTemplate ProceedApplicationConsentToDIPTemplate { get; set; } 
        
        public RequestToRemoveRecordToProvider RequestToRemoveRecordToProvider { get; set; }
        public Removal2iCheckToDSITTemplate Removal2iCheckToDSITTemplate { get; set; }
        public RecordRemovalRequestSentConfirmationToDSIT RecordRemovalRequestSentConfirmationToDSIT { get; set; }

        public RequestToRemoveServiceToProvider RequestToRemoveServiceToProvider { get;set; }

        public RemovalRequestForApprovalToDSIT RemovalRequestForApprovalToDSIT { get; set; }

        public RequestToRemoveServiceNotificationToDSIT RequestToRemoveServiceNotificationToDSIT { get; set; }

        public ServiceRemovedConfirmedToCabOrProvider ServiceRemovedConfirmedToCabOrProvider { get; set; }
        public ServiceRemovedToDSIT ServiceRemovedToDSIT { get; set; }

        public RecordRemovedToDSIT RecordRemovedToDSIT { get; set; }
        public RecordRemovedConfirmedToCabOrProvider RecordRemovedConfirmedToCabOrProvider { get; set; }

        public RemoveService2iCheckToDSIT RemoveService2iCheckToDSIT { get; set; }

        public ServiceRemovalRequestCreated ServiceRemovalRequestCreated { get; set; }
        public EditProviderRequestTemplate EditProviderRequestTemplate { get; set; }
        public EditServiceRequestTemplate EditServiceRequestTemplate { get; set; }
        public EditServiceRequestConfirmationTemplate EditServiceRequestConfirmationTemplate { get; set; }
        public EditProviderRequestConfirmationTemplate EditProviderRequestConfirmationTemplate { get; set; }
        public CancelServiceRemovalRequestToDSIT CancelServiceRemovalRequestToDSIT { get; set; }
        public CancelServiceRemovalRequestToProvider CancelServiceRemovalRequestToProvider { get; set; }
        public OpeningLoopProviderReminderToDSIT OpeningLoopProviderReminderToDSIT { get; set; }
        public ClosingLoopProviderReminderToDSIT ClosingLoopProviderReminderToDSIT { get; set; }
        public CabTransferConfirmationToDSIT CabTransferConfirmationToDSIT { get; set; }
        public CabTransferConfirmationToCAB CabTransferConfirmationToCAB { get; set; }
        public CabTransferCancelledToDSIT CabTransferCancelledToDSIT { get; set; }
        public CabTransferCancelledToCAB CabTransferCancelledToCAB { get; set; }

    }
}
