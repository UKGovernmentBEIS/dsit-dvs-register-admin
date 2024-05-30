using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum CertificateInfoStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Received")]
        Received = 1,
        [Description("In review")]
        InReview = 2,
        [Description("Approved for publication")]
        Approved = 3,
        [Description("Rejected")]
        Rejected = 4,
        [Description("Expired")]
        Expired = 5


    }
}
