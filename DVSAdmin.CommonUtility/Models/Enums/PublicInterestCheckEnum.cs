﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum PublicInterestCheckEnum
    {

        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last    
        [Description("In primary review")]
        InPrimaryReview = 1,
        [Description("Primary check failed")]
        PrimaryCheckFailed = 2,
        [Description("Primary check passed")]
        PrimaryCheckPassed = 3,
        [Description("Sent back by second reviewer")]
        SentBackBySecondReviewer = 4,
        [Description("Application rejected")]
        ApplicationRejected = 5,
        [Description("Application approved")]
        ApplicationApproved = 6
    }
}