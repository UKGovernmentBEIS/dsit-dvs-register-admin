using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RegManagementService : IRegManagementService
    {
        private readonly IRegManagementRepository regManagementRepository;
        private readonly ICertificateReviewRepository certificateReviewRepository;
        private readonly IMapper automapper;
        private readonly ILogger<PreRegistrationReviewService> logger;
        private readonly IEmailSender emailSender;

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper,
          ILogger<PreRegistrationReviewService> logger, IEmailSender emailSender, ICertificateReviewRepository certificateReviewRepository)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;
            this.logger = logger;
            this.emailSender = emailSender;
            this.certificateReviewRepository = certificateReviewRepository;
        }        
         public async Task<List<ProviderDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();
            return automapper.Map<List<ProviderDto>>(providers);
        }

        public async Task<ProviderDto> GetProviderDetails(int providerId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderDto providerDto = automapper.Map<ProviderDto>(provider);
            return providerDto;
        }

        public async Task<ProviderDto> GetProviderWithServiceDeatils(int providerId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderDto providerDto = automapper.Map<ProviderDto>(provider);

            var roles = await certificateReviewRepository.GetRoles();
            List<RoleDto> roleDtos = automapper.Map<List<RoleDto>>(roles);

            var identityProfiles = await certificateReviewRepository.GetIdentityProfiles();
            List<IdentityProfileDto> identityProfileDtos = automapper.Map<List<IdentityProfileDto>>(identityProfiles);

            var schemes = await certificateReviewRepository.GetSupplementarySchemes();
            List<SupplementarySchemeDto> supplementarySchemeDtos = automapper.Map<List<SupplementarySchemeDto>>(schemes);

           List<CertificateInformationDto> certificateInformationDtos = automapper.Map<List<CertificateInformationDto>>(provider.CertificateInformation);

            foreach (var item in certificateInformationDtos)
            {
                var roleIds = item.CertificateInfoRoleMapping.Select(mapping => mapping.RoleId);
                item.Roles = roleDtos.Where(x => roleIds.Contains(x.Id)).ToList();
                var identityProfileids = item.CertificateInfoIdentityProfileMapping.Select(mapping => mapping.IdentityProfileId);
                item.IdentityProfiles = identityProfileDtos.Where(x => identityProfileids.Contains(x.Id)).ToList();
                var schemeids = item.CertificateInfoSupSchemeMappings?.Select(x => x.SupplementarySchemeId);
                if (schemeids!=null && schemeids.Count() > 0)
                    item.SupplementarySchemes = supplementarySchemeDtos.Where(x => schemeids.Contains(x.Id)).ToList();
            }

            providerDto.CertificateInformation = certificateInformationDtos;
            return providerDto;
        }

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string userEmail)
        {
            GenericResponse genericResponse = await regManagementRepository.UpdateServiceStatus(serviceIds, providerId, userEmail, CertificateInfoStatusEnum.Published);
            ProviderStatusEnum providerStatus = ProviderStatusEnum.Published;
            Provider provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderStatusEnum currentStatus = provider.ProviderStatus;// keep current status for log
            List<CertificateInformation> serviceList = await certificateReviewRepository.GetCertificateInformationListByProvider(providerId);
            PreRegistration preRegistration = provider.PreRegistration;
            string verifiedCab = serviceList[0].CreatedBy??string.Empty;
            string services = string.Join("\r", serviceList.Where(item => serviceIds.Contains(item.Id)).Select(x => x.ServiceName.ToString()).ToArray())??string.Empty;
            //If no service is currently published AND one service status = Ready to publish:
            //Then provider status = Action required            
            if (serviceList.All(item => item.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish))
            {
                providerStatus = ProviderStatusEnum.ActionRequired;
            }
            //If all service status = Published:
            //Then provider status = Published
            if (serviceList.All(item => item.CertificateInfoStatus == CertificateInfoStatusEnum.Published))
            {
                providerStatus = ProviderStatusEnum.Published;
            }

            //If at least one service status = Published AND one service status = Ready to publish:
            //Then provider = Published – action required.
            if (serviceList.Any(item => item.CertificateInfoStatus == CertificateInfoStatusEnum.Published)  &&
             serviceList.Any(item => item.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish))
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
                await regManagementRepository.SavePublishRegisterLog(registerPublishLog);

                await emailSender.SendServicePublishedToDIP(preRegistration?.FullName??string.Empty, services, preRegistration?.Email??string.Empty);
                if(!string.IsNullOrEmpty(preRegistration?.SponsorEmail) && !string.IsNullOrEmpty(preRegistration?.SponsorFullName))
                {
                    await emailSender.SendServicePublishedToDIP(preRegistration.SponsorFullName, services, preRegistration.SponsorEmail);
                }
                await emailSender.SendServicePublishedToCAB(verifiedCab, services, verifiedCab);
                await emailSender.SendServicePublishedToDSIT(preRegistration?.URN??string.Empty, services);  
            }

            return genericResponse;
        }
    }
}
