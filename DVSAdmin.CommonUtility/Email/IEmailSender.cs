namespace DVSAdmin.CommonUtility.Email
{
    public interface IEmailSender
    {

        public Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress);
        public Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress);
        //PI Check
        public Task<bool> SendPrimaryCheckPassConfirmationToDSIT(string companyName, string serviceName,string expirationDate);
        public Task<bool> SendPrimaryCheckFailConfirmationToDSIT(string companyName, string serviceName, string expirationDate);
        public Task<bool> SendPrimaryCheckRoundTwoConfirmationToDSIT(string companyName, string serviceName, string expirationDate);
        public Task<bool> SendApplicationRejectedToDIP(string recipientName, string emailAddress);
        public Task<bool> SendApplicationRejectedConfirmationToDSIT(string companyName, string serviceName);
        public Task<bool> SendApplicationApprovedToDSIT(string companyName, string serviceName);      
        //Certificate review

        public Task<bool> SendCertificateInfoApprovedToCab(string recipientName, string companyName, string serviceName, string emailAddress);
        public Task<bool> SendCertificateInfoApprovedToDSIT(string companyName, string serviceName);
        public Task<bool> SendCertificateInfoRejectedToCab(string recipientName, string companyName, string serviceName, string rejectionCategory, string rejectionComments, string emailAddress);
        public Task<bool> SendCertificateInfoRejectedToDSIT(string companyName, string serviceName, string rejectionCategory, string rejectionComments);        public Task<bool> SendApplicationRestroredToDSIT();
        public Task<bool> SendProceedApplicationConsentToDIP(string companyName, string serviceName, string companyNumber, string companyAddress, string publicContactEmail, string publicPhoneNumber, string consentLink, List<string> emailAddress);        

        //Consent - closing loop
        public Task<bool> SendConsentToPublishToDIP(string companyName, string serviceName, string recipientName, string consentLink, string emailAddress);
        public Task<bool> SendAgreementToPublishToDIP(string companyName, string serviceName, string recipientName, string emailAddress);
        public Task<bool> SendAgreementToPublishToDSIT(string companyName, string serviceName);

        //reg - management
        public Task<bool> SendServicePublishedToDIP(string recipientName, string serviceName, string companyName, string emailAddress);
        public Task<bool> SendServicePublishedToCAB(string recipientName, string serviceName, string companyName, string emailAddress);
        public Task<bool> SendServicePublishedToDSIT(string companyName, string serviceName);
    }
}
