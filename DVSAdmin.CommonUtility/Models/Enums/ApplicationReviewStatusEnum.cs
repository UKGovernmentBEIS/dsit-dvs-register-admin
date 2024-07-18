﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models.Enums
{
    public enum ApplicationReviewStatusEnum
    {

        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Received")]
        Received = 1,
        [Description("In primary review")]
        InPrimaryReview = 2,
        [Description("Primary check failed")]
        PrimaryCheckFailed = 3,
        [Description("Primary check passed")]
        PrimaryCheckPassed = 4,
        [Description("Sent back by second reviewer")]
        SentBackBySecondReviewer = 5,
        [Description("Application rejected")]
        ApplicationRejected = 6,
        [Description("Application approved")]
        ApplicationApproved = 7
    }
}
