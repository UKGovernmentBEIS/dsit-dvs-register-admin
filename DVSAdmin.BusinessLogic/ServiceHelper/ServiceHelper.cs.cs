﻿using DVSAdmin.CommonUtility.Models;
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
                    service.ServiceStatus != ServiceStatusEnum.SavedAsDraft)
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
                    case ServiceStatusEnum.AwaitingRemovalConfirmation:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                    case ServiceStatusEnum.UpdatesRequested:
                        return ProviderStatusEnum.UpdatesRequested;
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
    }
}
