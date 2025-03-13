using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Validations;

namespace DVSAdmin.Models
{
    public class ServiceRemovalViewModel
    {
        public int  ServiceId { get; set; }
        public int ProviderId { get; set; }

        [RequiredEnumValue(ErrorMessage = "Select a service reason for removal")]
        public ServiceRemovalReasonEnum ServiceRemovalReason { get; set; }
    }
}
