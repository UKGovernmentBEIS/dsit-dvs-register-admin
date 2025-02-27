using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
            List<ProviderProfileDto> providersList = automapper.Map<List<ProviderProfileDto>>(providers);

            providersList = providersList.Select(providerDto =>
            {
                providerDto.Services = providerDto.Services
                    .Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                                s.ServiceStatus == ServiceStatusEnum.Published ||
                                s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                                s.ServiceStatus == ServiceStatusEnum.Removed ||
                                s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation||
                                s.ServiceStatus == ServiceStatusEnum.UpdatesRequested)
                    .ToList();
                return providerDto;
            }).ToList();

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
            providerDto.Services = providerDto.Services.
                Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish || 
                s.ServiceStatus == ServiceStatusEnum.Published || 
                s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation || 
                s.ServiceStatus == ServiceStatusEnum.Removed || 
                s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation||
                s.ServiceStatus == ServiceStatusEnum.UpdatesRequested).ToList();
            
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

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerProfileId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await regManagementRepository.UpdateServiceStatus(serviceIds, providerProfileId, ServiceStatusEnum.Published, loggedInUserEmail);
            ProviderStatusEnum providerStatus = ProviderStatusEnum.Published;
            ProviderProfile providerProfile = await regManagementRepository.GetProviderDetails(providerProfileId);
            ProviderStatusEnum currentStatus = providerProfile.ProviderStatus;// keep current status for log
            List<Service> serviceList = await certificateReviewRepository.GetServiceListByProvider(providerProfileId);        
            string verifiedCab = providerProfile.CabUser.CabEmail;
            string services = string.Join("\r", serviceList.Where(item => serviceIds.Contains(item.Id)).Select(x => x.ServiceName.ToString()).ToArray())??string.Empty;
            //If no service is currently published AND one service status = Ready to publish:
            //Then provider status = Action required            
            if (serviceList.All(item => item.ServiceStatus == ServiceStatusEnum.ReadyToPublish))
            {
                providerStatus = ProviderStatusEnum.ReadyToPublish;
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
                providerStatus = ProviderStatusEnum.ReadyToPublishNext;
            }

            genericResponse = await regManagementRepository.UpdateProviderStatus(providerProfileId,  providerStatus, loggedInUserEmail);
         
            if(genericResponse.Success)
            {
                //insert provider log
                RegisterPublishLog registerPublishLog = new();
                registerPublishLog.ProviderProfileId = providerProfileId;
                registerPublishLog.CreatedTime = DateTime.UtcNow;
                registerPublishLog.ProviderName = providerProfile.TradingName;
                registerPublishLog.Services = services;
                if (currentStatus == ProviderStatusEnum.ReadyToPublish) //Action required will be the status just before publishing provider for first time - which is updated through consent
                {
                    registerPublishLog.Description = "First published";
                }
                else // else status will be Published- Action Required 
                {
                    registerPublishLog.Description = serviceIds.Count +  " new services included";
                }
                 await regManagementRepository.SavePublishRegisterLog(registerPublishLog,  loggedInUserEmail);  

              
                await emailSender.SendServicePublishedToDIP(providerProfile.PrimaryContactFullName, services, providerProfile.RegisteredName, providerProfile.PrimaryContactEmail);
                await emailSender.SendServicePublishedToDIP(providerProfile.SecondaryContactFullName, services, providerProfile.RegisteredName, providerProfile.SecondaryContactEmail);               
                await emailSender.SendServicePublishedToCAB(verifiedCab, services, providerProfile.RegisteredName, verifiedCab);
                await emailSender.SendServicePublishedToDSIT(providerProfile.RegisteredName, services);
            }

            return genericResponse;
        }
        
    
    
    }
}
