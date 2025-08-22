using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum CertificateReviewEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
      
      
        [Description("Approved")]
        Approved = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Expired")]
        Expired = 4,
        [Description("Sent back to CAB")]
        AmendmentsRequired = 5,
        [Description("Received")]
        DeclinedByProvider = 6
    }
}
