using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RegManagementService : IRegManagementService
    {
        private readonly IRegManagementRepository regManagementRepository;
        private readonly IRemoveProviderService removeProviderService; 
        private readonly IMapper automapper;       
        private readonly RegManagementEmailSender emailSender;
      

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper,
          RegManagementEmailSender emailSender,
          IRemoveProviderService removeProviderService)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;            
            this.removeProviderService = removeProviderService;
        }
        public async Task<List<ProviderProfileDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();

            List<ProviderProfileDto> providersList = automapper.Map<List<ProviderProfileDto>>(providers);

            if(providersList!=null && providersList.Count()>0)
            {
                providersList = providersList.Select(providerDto =>
                {
                    providerDto.Services = ServiceHelper.FilterByServiceStatusAndLatestModifiedDate(providerDto.Services);                    
                    return providerDto;
                }).ToList();
            }
          

            return providersList;
        }

        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var service = await regManagementRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(service);
            return serviceDto;
        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerProfileId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerProfileId);            
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            providerDto.Services = ServiceHelper.FilterByServiceStatusAndLatestModifiedDate(providerDto.Services);
            return providerDto;
        }

        public async Task<ProviderProfileDto> GetProviderWithServiceDetails(int providerProfileId)
        {
            var provider = await regManagementRepository.GetProviderWithServiceDetails(providerProfileId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
          
            foreach (var service in providerDto.Services) 
            {
               var previousVersionList = providerDto.Services.Where(x => x.ServiceKey == service.ServiceKey && x.IsCurrent == false && x.ServiceStatus == ServiceStatusEnum.Published).ToList();
               service.HasPreviousPublishedVersion = previousVersionList !=null && previousVersionList.Count > 0;
            }
            return providerDto;
        }

        public async Task<List<string>> GetCabEmailListForServices(List<int> serviceIds)
        {
            return await regManagementRepository.GetCabEmailListForServices(serviceIds);
        }

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerProfileId, string loggedInUserEmail, List<string> cabEmails)
        {
            GenericResponse genericResponse = await regManagementRepository.UpdateServiceStatus(serviceIds, providerProfileId,  loggedInUserEmail);            
            ProviderProfile providerProfile = await regManagementRepository.GetProviderDetailsWithOutReviewDetails(providerProfileId);
            ProviderStatusEnum currentStatus = providerProfile.ProviderStatus;// keep current status for log
            List<Service> serviceList = await regManagementRepository.GetServiceListByProvider(providerProfileId);              
           
            
            string services = string.Join("\r", serviceList.Where(item => serviceIds.Contains(item.Id)).Select(x => x.ServiceName.ToString()).ToArray())??string.Empty;
        
            // update provider status based on priority
            genericResponse = await removeProviderService.UpdateProviderStatusByStatusPriority(providerProfileId, loggedInUserEmail, EventTypeEnum.RegisterManagement);

            if (genericResponse.Success)
            {
                //insert provider log
                RegisterPublishLog registerPublishLog = new();
                registerPublishLog.ProviderProfileId = providerProfileId;
                registerPublishLog.CreatedTime = DateTime.UtcNow;
                registerPublishLog.ProviderName = providerProfile.TradingName;
                registerPublishLog.Services = services;
              
                 await regManagementRepository.SavePublishRegisterLog(registerPublishLog,  loggedInUserEmail, serviceIds);  

              
                await emailSender.SendServicePublishedToDIP(providerProfile.PrimaryContactFullName, services, providerProfile.RegisteredName, providerProfile.PrimaryContactEmail);
                await emailSender.SendServicePublishedToDIP(providerProfile.SecondaryContactFullName, services, providerProfile.RegisteredName, providerProfile.SecondaryContactEmail);                              
                await emailSender.SendServicePublishedToDSIT(providerProfile.RegisteredName, services);
                foreach(var cabEmail in cabEmails)
                {
                    await emailSender.SendServicePublishedToCAB(cabEmail, services, providerProfile.RegisteredName, cabEmail);
                }
               
            }

            return genericResponse;
        }

        public async Task<List<ServiceDto>> GetServiceVersionList(int serviceKey)
        {
            var serviceList = await regManagementRepository.GetServiceVersionList(serviceKey);
            List<ServiceDto> services =  automapper.Map<List<ServiceDto>>(serviceList);
            return ServiceHelper.FilterByServiceStatus(services);

        }

       

    }
}
