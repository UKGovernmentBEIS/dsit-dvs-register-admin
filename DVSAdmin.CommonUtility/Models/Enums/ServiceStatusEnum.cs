﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models
{
    public enum ServiceStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Submitted")]
        Submitted = 1,
        [Description("Received")] // Status when provider consents to proceed application, opening loop
        Received = 2,
        [Description("Published")]
        Published = 3,
        [Description("Removed")]
        Removed = 4
    }
}
