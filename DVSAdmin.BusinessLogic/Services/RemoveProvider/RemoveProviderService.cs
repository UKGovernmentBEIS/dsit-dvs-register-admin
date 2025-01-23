using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RemoveProviderService : IRemoveProviderService
    {
        private readonly IRemoveProviderRepository removeProviderRepository;
        private readonly IMapper automapper;
        private readonly IEmailSender emailSender;
        private readonly IJwtService jwtService;


        public RemoveProviderService(IRemoveProviderRepository removeProviderRepository, IMapper automapper,
          IEmailSender emailSender, ICertificateReviewRepository certificateReviewRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.removeProviderRepository = removeProviderRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.jwtService = jwtService;
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
        public async Task<GenericResponse> RemoveServiceRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string>? dsitUserEmails, ServiceRemovalReasonEnum? serviceRemovalReason)
        {
            GenericResponse genericResponse = await removeProviderRepository.RemoveServiceRequest(providerProfileId, serviceIds, loggedInUserEmail, serviceRemovalReason);

            TeamEnum requstedBy = TeamEnum.DSIT;
            if (serviceRemovalReason != null && serviceRemovalReason == ServiceRemovalReasonEnum.ProviderRequestedRemoval)
            {
                requstedBy = TeamEnum.Provider;
            }

            if (genericResponse.Success)
            {
                ProviderProfile providerProfile = await removeProviderRepository.GetProviderDetails(providerProfileId);
                // update provider status

                ProviderStatusEnum providerStatus = GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
                genericResponse = await removeProviderRepository.UpdateProviderStatus(providerProfileId, providerStatus, loggedInUserEmail, EventTypeEnum.RemoveService);
                // save token for 2i check
                //Insert token details to db for further reference, if multiple services are removed, insert to mapping table

                TokenDetails tokenDetails = jwtService.GenerateToken(requstedBy == TeamEnum.DSIT ? "DSIT" : string.Empty);

                ICollection<RemoveTokenServiceMapping> removeTokenServiceMapping = [];
                foreach (var item in serviceIds)
                {
                    removeTokenServiceMapping.Add(new RemoveTokenServiceMapping { ServiceId = item });
                }

                RemoveProviderToken removeProviderToken = new()
                {
                    ProviderProfileId = providerProfileId,
                    Token = tokenDetails.Token,
                    TokenId = tokenDetails.TokenId,
                    RemoveTokenServiceMapping = removeTokenServiceMapping,
                    CreatedTime = DateTime.UtcNow
                };
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