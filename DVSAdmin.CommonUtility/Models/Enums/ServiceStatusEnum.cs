﻿using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel;
using System.Reflection;

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
        [Description("Ready to publish")]// Status when provider consents to publish application, closing the loop
        ReadyToPublish = 3,
        [Description("Published")]
        Published = 4,
        [Description("Removed")]
        Removed = 5,
        [Description("Awaiting removal confirmation")]
        AwaitingRemovalConfirmation = 6,
        [Description("Removal requested by CAB")]
        CabAwaitingRemovalConfirmation = 7,
        [Description("Saved as draft")]
        SavedAsDraft = 8
    }

    public static class ServiceStatusEnumExtensions
    {
        
        public static string GetDescription(this ServiceStatusEnum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
   
}
