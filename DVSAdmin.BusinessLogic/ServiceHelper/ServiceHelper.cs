using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
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
                var priorityOrder = Helper.priorityOrderService;

                ServiceStatusEnum highestPriorityStatus = services
                   .Where(service =>
                    service.ServiceStatus == ServiceStatusEnum.Published ||
                    service.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                    service.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign ||
                    service.ServiceStatus == ServiceStatusEnum.Removed ||
                    service.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                    service.ServiceStatus == ServiceStatusEnum.UpdatesRequested ||
                    service.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation)
                   .Select(service => service.ServiceStatus)
                   .OrderBy(status => priorityOrder.IndexOf(status))
                   .FirstOrDefault();

                switch (highestPriorityStatus)
                {
                    case ServiceStatusEnum.CabAwaitingRemovalConfirmation:
                        return ProviderStatusEnum.CabAwaitingRemovalConfirmation;                   
                    case ServiceStatusEnum.UpdatesRequested:
                        return ProviderStatusEnum.UpdatesRequested;
                    case ServiceStatusEnum.AwaitingRemovalConfirmation:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                    case ServiceStatusEnum.Published:
                        return ProviderStatusEnum.Published;
                    case ServiceStatusEnum.PublishedUnderReassign:
                        return ProviderStatusEnum.PublishedUnderReassign;
                    case ServiceStatusEnum.RemovedUnderReassign:
                        return ProviderStatusEnum.RemovedUnderReassign;
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
                filteredServices = services.Where(s => 
                                    s.ServiceStatus == ServiceStatusEnum.Published ||
                                    s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                                    s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.Removed ||
                                    s.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign ||
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
                filteredServices = services.Where(s => 
                                    s.ServiceStatus == ServiceStatusEnum.Published ||
                                    s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                                    s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                                    s.ServiceStatus == ServiceStatusEnum.Removed ||
                                    s.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign ||
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
