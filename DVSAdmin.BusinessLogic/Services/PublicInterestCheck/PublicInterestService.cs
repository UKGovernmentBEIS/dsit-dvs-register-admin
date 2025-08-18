using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class PublicInterestService : IPublicInterestCheckService
    {

        private readonly IPublicInterestCheckRepository publicInterestCheckRepository;        
        private readonly IConsentRepository consentRepository;
        private readonly IMapper automapper;
        private readonly PICheckEmailSender emailSender;
        private readonly IRemoveProviderService removeProviderService;
        private readonly IConfiguration configuration;



        public PublicInterestService(IPublicInterestCheckRepository publicInterestCheckRepository, IMapper automapper,
          PICheckEmailSender emailSender, IConsentRepository consentRepository, IRegManagementService regManagementService, 
          IRemoveProviderService removeProviderService, IConfiguration configuration)
        {
            this.publicInterestCheckRepository = publicInterestCheckRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.consentRepository = consentRepository;            
            this.removeProviderService = removeProviderService;
            this.configuration = configuration;

        }
        public async Task<List<ServiceDto>> GetPICheckList()
        {
            var publicinterestchecks = await publicInterestCheckRepository.GetPICheckList();
            return automapper.Map<List<ServiceDto>>(publicinterestchecks);
        }

        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var service = await publicInterestCheckRepository.GetServiceDetails(serviceId);
            return automapper.Map<ServiceDto>(service);
        }
        public async Task<ServiceDto> GetServiceDetailsForPublishing(int serviceId)
        {
            var service = await publicInterestCheckRepository.GetServiceDetailsForPublishing(serviceId);
            return automapper.Map<ServiceDto>(service);
        }

        public async Task<ServiceDto> GetServiceDetailsWithMappings(int serviceId)
        {
            var certificateInfo = await publicInterestCheckRepository.GetServiceDetailsWithMappings(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(certificateInfo);
            return serviceDto;
        }
        public async Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType, string loggedInUserEmail)
        {
            ServiceDto service = await GetServiceDetails(publicInterestCheckDto.ServiceId);

            PublicInterestCheck publicInterestCheck = new();
            automapper.Map(publicInterestCheckDto, publicInterestCheck);
            GenericResponse genericResponse = await publicInterestCheckRepository.SavePublicInterestCheck(publicInterestCheck, reviewType, loggedInUserEmail);
           
            if (genericResponse.Success)
            {                


                DateTime expirationdate = Convert.ToDateTime(service.ModifiedTime).AddDays(Constants.DaysLeftToCompletePICheck);
                string expirationDate = Helper.GetLocalDateTime(expirationdate, "d MMM yyyy h:mm tt");
                if (reviewType == ReviewTypeEnum.PrimaryCheck)
                {
                    if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed)
                    {
                        await emailSender.SendPrimaryCheckPassConfirmationToDSIT(service.Provider.RegisteredName, service.ServiceName, expirationDate);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed)
                    {
                       await emailSender.SendPrimaryCheckFailConfirmationToDSIT(service.Provider.RegisteredName, service.ServiceName, expirationDate);
                    }

                }
                else if (reviewType == ReviewTypeEnum.SecondaryCheck)
                {

                    if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer)
                    {                        
                      await emailSender.SendPrimaryCheckRoundTwoConfirmationToDSIT(service.Provider.RegisteredName, service.ServiceName, expirationDate);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckFailed)
                    {
                        await emailSender.SendApplicationRejectedToDIP(service.ServiceName, service.Provider.PrimaryContactEmail);
                        await emailSender.SendApplicationRejectedToDIP(service.ServiceName, service.Provider.SecondaryContactEmail);
                        await emailSender.SendApplicationRejectedConfirmationToDSIT(service.Provider.RegisteredName, service.ServiceName);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckPassed)
                    {                   
                        

                        if (genericResponse.Success)
                        {
                            await emailSender.SendApplicationApprovedToDSIT(service.ServiceName, service.Provider.RegisteredName);

                            genericResponse = await UpdateServiceStatus(service.Id, service.ServiceName, service.Provider.Id, loggedInUserEmail, service.CabUser.CabEmail);
                        }
                    }
                }
            }
            return genericResponse;
        }

        private async Task<GenericResponse> UpdateServiceStatus(int serviceId, string serviceName, int providerProfileId, string loggedInUserEmail, string cabEmail)
        {
            GenericResponse genericResponse = await publicInterestCheckRepository.UpdateServiceStatus(serviceId, loggedInUserEmail);
            ProviderProfile providerProfile = await publicInterestCheckRepository.GetProviderDetailsWithOutReviewDetails(providerProfileId);
        
            // update provider status based on priority
            genericResponse = await removeProviderService.UpdateProviderStatusByStatusPriority(providerProfileId, loggedInUserEmail, EventTypeEnum.PICheck);

            if (genericResponse.Success)
            {
                //insert provider log
                RegisterPublishLog registerPublishLog = new();
                registerPublishLog.ProviderProfileId = providerProfileId;
                registerPublishLog.CreatedTime = DateTime.UtcNow;
                registerPublishLog.ProviderName = providerProfile.TradingName;
                registerPublishLog.Services = serviceName;

                await publicInterestCheckRepository.SavePublishRegisterLog(registerPublishLog, loggedInUserEmail);

                string serviceLink = configuration["DvsRegisterLink"] + "register/provider-details?providerId=" + providerProfileId;

                await emailSender.SendServicePublishedToDIP(serviceName, serviceLink, providerProfile.PrimaryContactEmail);
                await emailSender.SendServicePublishedToDIP(serviceName, serviceLink, providerProfile.SecondaryContactEmail);
                await emailSender.SendServicePublishedToDSIT(providerProfile.RegisteredName, serviceName);
                await emailSender.SendServicePublishedToCAB(cabEmail, serviceName, providerProfile.RegisteredName, cabEmail);

            }

            return genericResponse;
        }

   


     
    }
}