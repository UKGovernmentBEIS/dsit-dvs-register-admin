using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum URNStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Expired")]
        Expired = 1
    }
}
