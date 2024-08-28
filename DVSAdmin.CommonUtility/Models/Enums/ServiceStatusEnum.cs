﻿using System.ComponentModel;

namespace DVSAdmin.CommonUtility.Models
{
    public enum ServiceStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Submitted")]
        Submitted = 1,      
        [Description("Published")]
        Published = 2,
        [Description("Removed")]
        Removed = 3
    }
}
