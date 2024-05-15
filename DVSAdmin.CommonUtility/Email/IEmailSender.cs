namespace DVSAdmin.CommonUtility.Email
{
    public interface IEmailSender
    {       
        public Task<bool> SendPrimaryCheckPassConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryCheckFailConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryCheckRoundTwoConfirmationToOfDia(string URN, string expirationDate);
        public Task<bool> SendPrimaryApplicationRejectedConfirmationToOfDia(string URN);
        public Task<bool> SendURNIssuedConfirmationToOfDia(string URN);
        public Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress);
        public Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress);       
        public Task<bool> SendApplicationRejectedToDIASP(string recipientName, string emailAddress);
        public Task<bool> SendApplicationApprovedToDIASP(string recipientName, string URN, string expiryDate, string emailAddress);

    }
}
