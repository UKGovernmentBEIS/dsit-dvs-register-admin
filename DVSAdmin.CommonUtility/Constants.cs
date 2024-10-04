namespace DVSAdmin.CommonUtility
{
    public static class Constants
    {
        public const string DbContextNull = "Db context is null";
        public const string DbConnectionFailed = "DB connection failed:";
        public const string DbConnectionSuccess = "DB connection success.";
        public const string ErrorPath = "service-error";
        public const int DaysLeftToComplete = 21;
        public const int DaysLeftToCompleteCertificateReview = 7;
        public const int DaysLeftToCompletePICheck = 21;    
        public const string IncorrectPassword = "Incorrect Password";
        public const int DaysLeftToPublish = 5;
        public const string IncorrectLoginDetails = "Enter a valid email address and password. After five incorrect attempts, your account will be temporarily locked";
        public const string InvalidCode = "Invalid verification code provided";
        public const string UserAccountExists = "User account already exists";
        public const string PrimaryCheckRejectErrorMessage = "Your decision to pass or fail this primary check must match with the selections. You have selected reject for {0}; to pass primary check this must be approved.";
        public const string PrimaryCheckApproveErrorMessage = "Your decision to pass or fail this primary check must match with the selections. You have selected approve for {0}.";


    }
}
