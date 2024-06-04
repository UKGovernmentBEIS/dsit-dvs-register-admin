using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{

    public enum CertificateReviewTypeEnum
    {
        [Description("Rejection")]
        Rejection = 0,
        [Description("Validation")]
        Validation = 1,
        [Description("InformationMatch")]
        InformationMatch = 2,
    }
}
