using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum ProviderStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Received")]
        Received = 1,
        [Description("Action required")]
        ActionRequired = 2,
        [Description("Ready to publish")]
        ReadyToPublish = 3,
        [Description("Published")]
        Published = 4
    }
}
