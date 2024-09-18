namespace DVSAdmin.CommonUtility.Email
{
    public interface IEmailSender
    {       
        //PI Check
        public Task<bool> SendPrimaryCheckPassConfirmationToDSIT(string companyName, string serviceName,string expirationDate);
        public Task<bool> SendPrimaryCheckFailConfirmationToDSIT(string companyName, string serviceName, string expirationDate);
        public Task<bool> SendPrimaryCheckRoundTwoConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryApplicationRejectedConfirmationToOfDia(string URN);
        public Task<bool> SendURNIssuedConfirmationToOfDia(string URN);
        public Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress);
        public Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress);       
        public Task<bool> SendApplicationRejectedToDIASP(string recipientName, string emailAddress);
        public Task<bool> SendApplicationApprovedToDIASP(string recipientName, string URN, string expiryDate, string emailAddress);

        //Certificate review

        public Task<bool> SendCertificateInfoApprovedToCab(string recipientName, string companyName, string serviceName, string emailAddress);
        public Task<bool> SendCertificateInfoApprovedToDSIT(string companyName, string serviceName);
        public Task<bool> SendCertificateInfoRejectedToCab(string recipientName, string companyName, string serviceName, string rejectionCategory, string rejectionComments, string emailAddress);
        public Task<bool> SendCertificateInfoRejectedToDSIT(string companyName, string serviceName, string rejectionCategory, string rejectionComments);
        public Task<bool> SendProceedApplicationConsentToDIP(string companyName, string serviceName, string companyNumber, string companyAddress, string publicContactEmail, string publicPhoneNumber, string consentLink, List<string> emailAddress);

        //Consent - closing loop
        public Task<bool> SendConsentToPublishToDIP(string companyName, string serviceName, string recipientName, string consentLink, string emailAddress);


        public Task<bool> SendAgreementToPublishToDIP(string recipientName, string emailAddress);
        public Task<bool> SendAgreementToPublishToDSIT(string URN, string serviceName);

        //reg - management
        public Task<bool> SendServicePublishedToDIP(string recipientName, string serviceName, string companyName, string emailAddress);
        public Task<bool> SendServicePublishedToCAB(string recipientName, string serviceName, string companyName, string emailAddress);
        public Task<bool> SendServicePublishedToDSIT(string companyName, string serviceName);
    }
}
