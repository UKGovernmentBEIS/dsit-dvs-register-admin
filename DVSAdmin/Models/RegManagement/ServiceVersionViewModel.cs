using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models.RegManagement
{
    public class ServiceVersionViewModel
    {
        public ServiceDto CurrentServiceVersion { get; set; }
        public List<ServiceDto> ServiceHistoryVersions { get; set; }
        public bool CanResendRemovalRequest { get; set; }
        public bool CanResendUpdateRequest { get; set; }

    }
}