using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.Models
{
    public class ViewModelHelper
    {
        public static List<string> GetCabsForProvider(List<ServiceDto> services)
        {
            return services .Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                    s.ServiceStatus == ServiceStatusEnum.Published ||
                    s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                    s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation ||
                    s.ServiceStatus == ServiceStatusEnum.UpdatesRequested)
                   .Select(s => s.CabUser.Cab.CabName).Distinct().ToList();
        }
    }
}
