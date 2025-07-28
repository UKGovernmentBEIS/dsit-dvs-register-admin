using DVSAdmin.BusinessLogic.Models.CertificateReview;

namespace DVSAdmin.BusinessLogic.Models.TrustFramework
{
    public class QualityLevelDtoWithFlagDto
    {
        public bool? HasGpg44 { get; set; }
        public List<QualityLevelDto>? QualityLevels { get; set; }
    }
}
