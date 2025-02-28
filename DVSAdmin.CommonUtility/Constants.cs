﻿namespace DVSAdmin.CommonUtility
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
        public const string CabHasWithdrawnCertificate = "The CAB has withdrawn the certificate for the service";
        public const string RegisteredNameExistsError = "The registered name you have entered already exists";
        public const string ConformityIssueDayError = "The certificate of conformity issue date must include a day";
        public const string ConformityIssueMonthError = "The certificate of conformity issue date must include a month";
        public const string ConformityIssueYearError = "The certificate of conformity issue date must include a year";
        public const string ConformityIssuePastDateError = "The certificate of conformity issue date must be today or in the past";
        public const string ConformityIssueDateInvalidError = "The certificate of conformity issue date must be a real date";
        public const string ConformityExpiryDayError = "The certificate of conformity expiry date must include a day";
        public const string ConformityExpiryMonthError = "The certificate of conformity expiry date must include a month";
        public const string ConformityExpiryYearError = "The certificate of conformity expiry date must include a year";
        public const string ConformityExpiryDateError = "The certificate of conformity expiry date must be today or in the past";
        public const string ConformityMaxExpiryDateError = "The certificate of conformity expiry date must not be more than 2 years 60 days after the date of issue";
        public const string ConformityExpiryDateInvalidError = "The certificate of conformity expiry date must be a real date";
        public const string ConformityIssueDateExpiryDateError = "The certificate of conformity expiry date cannot be before issue date";
        public const string ConformityExpiryPastDateError = "The certificate of conformity expiry date must be in the future";


    }
}
