﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum ProviderStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Unpublished")]
        Unpublished = 1,
        [Description("Action required")]
        ActionRequired = 2,
        [Description("Ready to publish")]
        ReadyToPublish = 3,
        [Description("Published")]
        Published = 4,
        [Description("Published - action required")]
        PublishedActionRequired = 5,
        [Description("Removed from register")]
        RemovedFromRegister = 6
    }
}
