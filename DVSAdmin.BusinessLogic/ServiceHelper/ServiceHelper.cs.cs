using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
    public class ServiceHelper
    {
        public static ProviderStatusEnum GetProviderStatus(ICollection<Service> services, ProviderStatusEnum currentStatus)
        {
            ProviderStatusEnum providerStatus = currentStatus;
            if (services != null && services.Count > 0)
            {
                var priorityOrder = new List<ServiceStatusEnum>
                    {
                        ServiceStatusEnum.CabAwaitingRemovalConfirmation,
                        ServiceStatusEnum.ReadyToPublish,
                        ServiceStatusEnum.UpdatesRequested,
                        ServiceStatusEnum.AwaitingRemovalConfirmation,
                        ServiceStatusEnum.Published,
                        ServiceStatusEnum.Removed
                    };

                ServiceStatusEnum highestPriorityStatus = services
                   .Where(service => service.ServiceStatus > ServiceStatusEnum.Received &&
                    service.ServiceStatus < ServiceStatusEnum.SavedAsDraft)
                   .Select(service => service.ServiceStatus)
                   .OrderBy(status => priorityOrder.IndexOf(status))
                   .FirstOrDefault();

                switch (highestPriorityStatus)
                {
                    case ServiceStatusEnum.CabAwaitingRemovalConfirmation:
                        return ProviderStatusEnum.CabAwaitingRemovalConfirmation;
                    case ServiceStatusEnum.ReadyToPublish:
                        bool hasPublishedServices = services.Any(service => service.ServiceStatus == ServiceStatusEnum.Published);
                        return hasPublishedServices ? ProviderStatusEnum.ReadyToPublishNext : ProviderStatusEnum.ReadyToPublish;
                    case ServiceStatusEnum.UpdatesRequested:
                        return ProviderStatusEnum.UpdatesRequested;
                    case ServiceStatusEnum.AwaitingRemovalConfirmation:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                    case ServiceStatusEnum.Published:
                        return ProviderStatusEnum.Published;
                    case ServiceStatusEnum.Removed:
                        return ProviderStatusEnum.RemovedFromRegister;
                    default:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                }
            }
            return providerStatus;
        }


        public static List<ServiceDto> FilterByServiceStatus(List<ServiceDto> services)
        {
            List<ServiceDto> filteredServices = [];
            if (services != null && services.Count > 0)
            {
                filteredServices = services.Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                                    s.ServiceStatus == ServiceStatusEnum.Published ||
                                    s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.Removed ||
                                    s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.UpdatesRequested).ToList();
               

            }
            return filteredServices;
        }

        public static List<ServiceDto> FilterByServiceStatusAndLatestModifiedDate(List<ServiceDto> services)
        {
            List<ServiceDto> filteredServices = [];
            List<ServiceDto> filteredServicesByDate = [];
            if (services != null && services.Count > 0)
            {
                filteredServices = services.Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                                    s.ServiceStatus == ServiceStatusEnum.Published ||
                                    s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.Removed ||
                                    s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.UpdatesRequested).ToList();


                filteredServicesByDate = filteredServices.GroupBy(s => s.ServiceKey) // Group by ServiceKey
                .Select(g => g.OrderByDescending(s => s.ModifiedTime).FirstOrDefault()).Where(s=>s!=null) // Filter out null values // Select the latest modified date
                .ToList();
             
            }
            return filteredServicesByDate;
        }


    }
}
