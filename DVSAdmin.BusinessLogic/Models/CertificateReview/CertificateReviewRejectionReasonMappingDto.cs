namespace DVSAdmin.BusinessLogic.Models.CertificateReview
{
    public class CertificateReviewRejectionReasonMappingDto
    {
        
        public int Id { get; set; }      
        public int CetificateReviewId { get; set; }
        public CertificateReviewDto CetificateReview { get; set; }        
        public int CertificateReviewRejectionReasonId { get; set; }
        public CertificateReviewRejectionReasonDto CertificateReviewRejectionReason { get; set; }
    }
}
