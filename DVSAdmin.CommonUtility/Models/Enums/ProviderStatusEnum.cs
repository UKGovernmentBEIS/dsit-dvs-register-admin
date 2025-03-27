﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum ProviderStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Unpublished")]
        Unpublished = 1,
        [Description("Ready to publish")] // Action required
        ReadyToPublish = 2,       
        [Description("Published")]
        Published = 3,
        [Description("Ready to publish")] //Published - action required
        ReadyToPublishNext = 4,
        [Description("Removed from register")]
        RemovedFromRegister = 5,
        [Description("Removal request sent")]
        AwaitingRemovalConfirmation = 6,
        [Description("Removal requested by CAB")]
        CabAwaitingRemovalConfirmation = 7,
        [Description("Updates requested")]
        UpdatesRequested = 8

    }
}
