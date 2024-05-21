﻿namespace DVSAdmin.CommonUtility.Models
{
    public class GovUkNotifyConfiguration
    {
        public const string ConfigSection = "GovUkNotify";
        public string ApiKey { get; set; }
        public string OfDiaEmailId { get; set; }
        public string LoginLink { get; set; }
        public string PreRegLink { get; set; }

        public PassPrimaryCheckTemplate PassPrimaryCheckTemplate { get;set; }
        public FailPrimaryCheckTemplate FailPrimaryCheckTemplate { get; set; }
        public PrimaryCheckRoundTwoTemplate PrimaryCheckRoundTwoTemplate { get; set;}
        public ApplicationRejectedTemplate ApplicationRejectedTemplate { get; set; }
        public IssueURNConfirmationTemplate IssueURNConfirmationTemplate { get; set; }
        public AccountCreatedTemplate AccountCreatedTemplate { get; set; }
        public FailedLoginAttemptTemplate FailedLoginAttemptTemplate { get; set; } 
        public ApplicationRejectedConfirmationTemplate ApplicationRejectedConfirmationTemplate { get; set; }
        public ApplicationApprovedConfirmationTemplate ApplicationApprovedConfirmationTemplate { get; set; }
    }
}