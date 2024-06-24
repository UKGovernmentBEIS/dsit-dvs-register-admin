namespace DVSAdmin.CommonUtility.Email
{
    public interface IEmailSender
    {       
        //Pre-reg review
        public Task<bool> SendPrimaryCheckPassConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryCheckFailConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryCheckRoundTwoConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryApplicationRejectedConfirmationToOfDia(string URN);
        public Task<bool> SendURNIssuedConfirmationToOfDia(string URN);
        public Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress);
        public Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress);       
        public Task<bool> SendApplicationRejectedToDIASP(string recipientName, string emailAddress);
        public Task<bool> SendApplicationApprovedToDIASP(string recipientName, string URN, string expiryDate, string emailAddress);

        //Certificate review

        public Task<bool> SendCertificateInfoApprovedToCab(string recipientName, string URN, string serviceName, string emailAddress);
        public Task<bool> SendCertificateInfoApprovedToDSIT(string URN, string serviceName);
        public Task<bool> SendCertificateInfoRejectedToCab(string recipientName, string URN, string serviceName, string emailAddress);
        public Task<bool> SendCertificateInfoRejectedToDSIT(string URN, string serviceName, string rejectionCategory, string rejectionComments);
        public Task<bool> SendConsentToPublishToDIP(string URN, string serviceName, string recipientName, string emailAddress, string consentLink);
       public Task<bool> SendConsentToPublishToAdditionalContact(string URN, string serviceName, string recipientName, string emailAddress);

    }
}
