using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RemoveProviderService : IRemoveProviderService
    {
        private readonly IRemoveProviderRepository removeProviderRepository;
        private readonly IMapper automapper;
        private readonly IEmailSender emailSender;



        public RemoveProviderService(IRemoveProviderRepository removeProviderRepository, IMapper automapper,
          IEmailSender emailSender)
        {
            this.removeProviderRepository = removeProviderRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;           
        }
        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var service = await removeProviderRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(service);
            return serviceDto;
        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerProfileId)
        {
            var provider = await removeProviderRepository.GetProviderDetails(providerProfileId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            return providerDto;
        }
        public async Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string>? dsitUserEmails)
        {
            GenericResponse genericResponse = await removeProviderRepository.RemoveServiceRequestByCab(providerProfileId, serviceIds, loggedInUserEmail);
                       
            if (genericResponse.Success)
            {
                ProviderProfile providerProfile = await removeProviderRepository.GetProviderDetails(providerProfileId);
                // update provider status

                ProviderStatusEnum providerStatus = GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
                genericResponse = await removeProviderRepository.UpdateProviderStatus(providerProfileId, providerStatus, loggedInUserEmail, EventTypeEnum.RemoveServiceRequestedByCab);
                // save token for 2i check

                //To do : emails
             
            }

            return genericResponse;
        }

        private ProviderStatusEnum GetProviderStatus(ICollection<Service> services, ProviderStatusEnum currentStatus)
        {
            ProviderStatusEnum providerStatus = currentStatus;
            if (services != null && services.Count > 0)
            {

                if (services.All(service => service.ServiceStatus == ServiceStatusEnum.Removed))
                {
                    providerStatus = ProviderStatusEnum.RemovedFromRegister;
                    return providerStatus;
                }

                var priorityOrder = new List<ServiceStatusEnum>
                    {
                        ServiceStatusEnum.CabAwaitingRemovalConfirmation,
                        ServiceStatusEnum.ReadyToPublish,
                        ServiceStatusEnum.AwaitingRemovalConfirmation,
                        ServiceStatusEnum.Published,
                        ServiceStatusEnum.Removed
                    };

                ServiceStatusEnum highestPriorityStatus = services.Select(service => service.ServiceStatus).OrderBy(status => priorityOrder.IndexOf(status)).FirstOrDefault();


                switch (highestPriorityStatus)
                {
                    case ServiceStatusEnum.CabAwaitingRemovalConfirmation:
                        return ProviderStatusEnum.CabAwaitingRemovalConfirmation;
                    case ServiceStatusEnum.ReadyToPublish:
                        bool hasPublishedServices = services.Any(service => service.ServiceStatus == ServiceStatusEnum.Published);
                        return hasPublishedServices ? ProviderStatusEnum.PublishedActionRequired : ProviderStatusEnum.ActionRequired;
                    case ServiceStatusEnum.AwaitingRemovalConfirmation:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                    case ServiceStatusEnum.Published:
                        return ProviderStatusEnum.Published;
                    default:
                        return ProviderStatusEnum.AwaitingRemovalConfirmation;
                }

            }
            return providerStatus;
        }
    }
}