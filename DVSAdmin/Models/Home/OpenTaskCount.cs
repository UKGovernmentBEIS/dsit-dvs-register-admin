namespace DVSAdmin.Models.Home
{
    public class OpenTaskCount
    {
        public int PendingCertificateReviews { get; set; }
        public int PendingPrimaryChecks { get; set; }
        public int PendingSecondaryChecks { get; set; }
        public int PendingUpdateAndRemovalRequests { get; set; }
    }
}
