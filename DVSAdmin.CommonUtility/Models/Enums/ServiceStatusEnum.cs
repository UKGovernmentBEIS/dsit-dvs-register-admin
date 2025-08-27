using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel;
using System.Reflection;

namespace DVSAdmin.CommonUtility.Models
{
    public enum ServiceStatusEnum
    {
        //Donot change order of the enum as the ids are used to save in database
        //New entries should be added at the last
        [Description("Received")]
        Submitted = 1,
        [Description("Received")] // Status when provider consents to proceed application, opening loop
        Received = 2,
        //[Description("Ready to publish")]// Status when provider consents to publish application, closing the loop
        //ReadyToPublish = 3,
        [Description("Published")]
        Published = 4,
        [Description("Removed from register")]
        Removed = 5,
        [Description("Removal request sent")]
        AwaitingRemovalConfirmation = 6,
        [Description("Removal requested by CAB")]
        CabAwaitingRemovalConfirmation = 7,
        [Description("Saved as draft")]
        SavedAsDraft = 8,
        [Description("Updates requested")]
        UpdatesRequested = 9,
        [Description("Sent back to CAB")]
        AmendmentsRequired = 10,
        [Description("Received")]
        Resubmitted = 11,
        [Description("Published, under reassignment")]
        PublishedUnderReassign = 12,
        [Description("Removed, under reassignment")]
        RemovedUnderReassign = 13
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
