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

                ProviderStatusEnum providerStatus = ServiceHelper.GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
                genericResponse = await removeProviderRepository.UpdateProviderStatus(providerProfileId, providerStatus, loggedInUserEmail, EventTypeEnum.RemoveServiceRequestedByCab);
                // save token for 2i check

                Service service = providerProfile.Services.Where(s => s.Id == serviceIds[0]).FirstOrDefault(); // only single service removal in current release
               if (genericResponse.Success && providerStatus == ProviderStatusEnum.RemovedFromRegister)
                {
                    await emailSender.SendRecordRemovedToDSIT(providerProfile.RegisteredName, service.ServiceName, service.RemovalReasonByCab);
                    await emailSender.RecordRemovedConfirmedToCabOrProvider(providerProfile.CabUser.CabEmail, providerProfile.CabUser.CabEmail, providerProfile.RegisteredName, service.ServiceName, service.RemovalReasonByCab);
                    await emailSender.RecordRemovedConfirmedToCabOrProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, providerProfile.RegisteredName, service.ServiceName, service.RemovalReasonByCab);
                    await emailSender.RecordRemovedConfirmedToCabOrProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, providerProfile.RegisteredName, service.ServiceName, service.RemovalReasonByCab);
                }
                else
                {
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.CabUser.CabEmail, service.CabUser.CabEmail, service.ServiceName, service.RemovalReasonByCab);//41/CAB + Provider/Service removed
                    await emailSender.ServiceRemovedToDSIT(service.ServiceName, service.RemovalReasonByCab); //42/DSIT/Service removed
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.Provider.PrimaryContactFullName, service.Provider.PrimaryContactEmail, service.ServiceName, service.RemovalReasonByCab);//41/CAB + Provider/Service removed
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.Provider.SecondaryContactFullName, service.Provider.SecondaryContactEmail, service.ServiceName, service.RemovalReasonByCab);//41/CAB + Provider/Service removed
                }
             
            }

            return genericResponse;
        }

       
    }
}