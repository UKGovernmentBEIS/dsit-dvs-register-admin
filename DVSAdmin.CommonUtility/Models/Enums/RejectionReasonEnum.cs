using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum RejectionReasonEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Failed due diligence check")]
        DiligenceCheck=1,
        [Description("Submitted incorrect information ")]
        IncorrectInfo=2,
    }
}
