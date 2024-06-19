namespace DVSAdmin.CommonUtility.Models
{
    public class CertificateInfoRejectedToDSITTemplate
    {
        public string Id { get; set; }       
        public string URN { get; set; }
        public string ServiceName { get; set; }
        public string RejectionCategory{ get; set; }
        public string RejectionComments { get; set; }
    }
}
