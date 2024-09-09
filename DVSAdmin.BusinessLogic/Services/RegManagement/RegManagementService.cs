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
        private readonly ICertificateReviewRepository certificateReviewRepository;
        private readonly IMapper automapper;       
        private readonly IEmailSender emailSender;

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper,
          IEmailSender emailSender, ICertificateReviewRepository certificateReviewRepository)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.certificateReviewRepository = certificateReviewRepository;
        }        
         public async Task<List<ProviderProfileDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();
            return automapper.Map<List<ProviderProfileDto>>(providers);
        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            return providerDto;
        }

        public async Task<ProviderProfileDto> GetProviderWithServiceDeatils(int providerId)
        {
            var provider = await regManagementRepository.GetProviderWithServiceDetails(providerId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);           
            return providerDto;
        }

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId)
        {
            GenericResponse genericResponse = await regManagementRepository.UpdateServiceStatus(serviceIds, providerId,ServiceStatusEnum.Published);
            ProviderStatusEnum providerStatus = ProviderStatusEnum.Published;
            ProviderProfile provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderStatusEnum currentStatus = provider.ProviderStatus;// keep current status for log
            List<Service> serviceList = await certificateReviewRepository.GetServiceListByProvider(providerId);
            //PreRegistration preRegistration = provider.PreRegistration;
            string verifiedCab = provider.CabUser.CabEmail;
            string services = string.Join("\r", serviceList.Where(item => serviceIds.Contains(item.Id)).Select(x => x.ServiceName.ToString()).ToArray())??string.Empty;
            //If no service is currently published AND one service status = Ready to publish:
            //Then provider status = Action required            
            if (serviceList.All(item => item.ServiceStatus == ServiceStatusEnum.ReadyToPublish))
            {
                providerStatus = ProviderStatusEnum.ActionRequired;
            }
            //If all service status = Published:
            //Then provider status = Published
            if (serviceList.All(item => item.ServiceStatus == ServiceStatusEnum.Published))
            {
                providerStatus = ProviderStatusEnum.Published;
            }

            //If at least one service status = Published AND one service status = Ready to publish:
            //Then provider = Published – action required.
            if (serviceList.Any(item => item.ServiceStatus == ServiceStatusEnum.Published)  &&
             serviceList.Any(item => item.ServiceStatus == ServiceStatusEnum.ReadyToPublish))
            {
                providerStatus = ProviderStatusEnum.PublishedActionRequired;
            }

            genericResponse = await regManagementRepository.UpdateProviderStatus(providerId,  providerStatus);
         
            if(genericResponse.Success)
            {
                //insert provider log
                RegisterPublishLog registerPublishLog = new RegisterPublishLog();
                registerPublishLog.ProviderId = providerId;
                registerPublishLog.CreatedTime = DateTime.UtcNow;
                registerPublishLog.ProviderName = provider.TradingName;
                registerPublishLog.Services = services;
                if (currentStatus == ProviderStatusEnum.ActionRequired) //Action required will be the status just before publishing provider for first time - which is updated through consent
                {
                    registerPublishLog.Description = "First published";
                }
                else // else status will be Published- Action Required 
                {
                    registerPublishLog.Description = serviceIds.Count +  " new services included";
                }
                // await regManagementRepository.SavePublishRegisterLog(registerPublishLog);  To DO after creating new table

                //TO Do : emails
                //await emailSender.SendServicePublishedToDIP(provider.PrimaryContactFullName, services, provider.PrimaryContactEmail);
                //await emailSender.SendServicePublishedToDIP(provider.SecondaryContactFullName, services, provider.SecondaryContactEmail);
                //await emailSender.SendServicePublishedToCAB(verifiedCab, services, verifiedCab);
                //await emailSender.SendServicePublishedToDSIT(preRegistration?.URN??string.Empty, services);
            }

            return genericResponse;
        }
    }
}
