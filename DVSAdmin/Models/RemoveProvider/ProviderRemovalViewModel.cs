using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Validations;

namespace DVSAdmin.Models
{
    public class ProviderRemovalViewModel
    {
        public int ProviderId { get; set; }

        [RequiredEnumValueAttribute(ErrorMessage = "Select a reason for removal")]
        public RemovalReasonsEnum RemovalReason { get; set; }
    }
}
