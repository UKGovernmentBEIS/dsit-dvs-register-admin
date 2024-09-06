using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Models.CertificateReview
{
    public class QualityLevelDto
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public QualityTypeEnum QualityType { get; set; }
    }
}
