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
    public class RegManagementService : IRegManagementService
    {
        private readonly IRegManagementRepository regManagementRepository;
        private readonly ICertificateReviewRepository certificateReviewRepository;
        private readonly IMapper automapper;       
        private readonly IEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper,
          IEmailSender emailSender, ICertificateReviewRepository certificateReviewRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.certificateReviewRepository = certificateReviewRepository;
            this.jwtService = jwtService;
            this.configuration = configuration;
        }        
         public async Task<List<ProviderProfileDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();
            return automapper.Map<List<ProviderProfileDto>>(providers);
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
            return providerDto;
        }

        public async Task<ProviderProfileDto> GetProviderWithServiceDetails(int providerProfileId)
        {
            var provider = await regManagementRepository.GetProviderWithServiceDetails(providerProfileId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);           
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

            genericResponse = await regManagementRepository.UpdateProviderStatus(providerProfileId,  providerStatus, loggedInUserEmail);
         
            if(genericResponse.Success)
            {
                //insert provider log
                RegisterPublishLog registerPublishLog = new RegisterPublishLog();
                registerPublishLog.ProviderProfileId = providerProfileId;
                registerPublishLog.CreatedTime = DateTime.UtcNow;
                registerPublishLog.ProviderName = providerProfile.TradingName;
                registerPublishLog.Services = services;
                if (currentStatus == ProviderStatusEnum.ActionRequired) //Action required will be the status just before publishing provider for first time - which is updated through consent
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

       


        public async Task<GenericResponse> RemoveServiceRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails,ServiceRemovalReasonEnum? serviceRemovalReason)
        {
            GenericResponse genericResponse = await regManagementRepository.RemoveServiceRequest( providerProfileId, serviceIds, loggedInUserEmail, serviceRemovalReason);
            
            TeamEnum requstedBy = TeamEnum.DSIT;
            if (serviceRemovalReason != null && serviceRemovalReason == ServiceRemovalReasonEnum.ProviderRequestedRemoval)
            {
                requstedBy = TeamEnum.Provider;
            }

            if (genericResponse.Success)
            {
                ProviderProfile providerProfile = await regManagementRepository.GetProviderDetails(providerProfileId);
                // update provider status

                ProviderStatusEnum providerStatus = GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
                genericResponse = await regManagementRepository.UpdateProviderStatus(providerProfileId, providerStatus,loggedInUserEmail,EventTypeEnum.RemoveService);
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
                genericResponse = await regManagementRepository.SaveRemoveProviderToken(removeProviderToken, TeamEnum.DSIT, EventTypeEnum.RemoveService, loggedInUserEmail);


                if (genericResponse.Success)
                {
                    var filteredServiceNames = providerProfile.Services.Where(s => serviceIds.Contains(s.Id)).Select(s => s.ServiceName).ToList();
                    string serviceNames = string.Join("\r", filteredServiceNames);
                    string reasonString = ServiceRemovalReasonEnumExtensions.GetDescription(serviceRemovalReason.Value);
                    if (requstedBy == TeamEnum.Provider)
                    {
                        string linkForEmailToProvider = configuration["DvsRegisterLink"] + "remove-provider/provider/provider-details?token=" + tokenDetails.Token;
                        await emailSender.SendRequestToRemoveServiceToProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, serviceNames,reasonString, linkForEmailToProvider);
                        await emailSender.SendRequestToRemoveServiceToProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, serviceNames, reasonString, linkForEmailToProvider);
                        

                        //await emailSender.SendRecordRemovalRequestConfirmationToDSIT(providerProfile.RegisteredName, serviceNames);
                    }
                    else if (requstedBy == TeamEnum.DSIT)
                    {

                        string linkForEmailToDSIT = configuration["DvsRegisterLink"] + "remove-provider/dsit/provider-details?token=" + tokenDetails.Token;
                        foreach (var email in dsitUserEmails)
                        {
                           await emailSender.SendRemoval2iCheckToDSIT(email, email, linkForEmailToDSIT, providerProfile.RegisteredName, serviceNames, reasonString);
                        }

                    }
                }               
            }

            return genericResponse;
        }




        public async Task<GenericResponse> RemoveProviderRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, RemovalReasonsEnum? reason)
        {
            GenericResponse genericResponse = await regManagementRepository.RemoveProviderRequest(providerProfileId, serviceIds, loggedInUserEmail, reason);

            TeamEnum requstedBy = reason == RemovalReasonsEnum.ProviderRequestedRemoval ? TeamEnum.Provider: TeamEnum.DSIT;           

            if (genericResponse.Success)
            {
                // save token for 2i check
                //Insert token details to db for further reference, if multiple services are removed, insert to mapping table

                TokenDetails tokenDetails = jwtService.GenerateToken(requstedBy == TeamEnum.DSIT ? "DSIT" : string.Empty);               

                RemoveProviderToken removeProviderToken = new()
                {
                    ProviderProfileId = providerProfileId,
                    Token = tokenDetails.Token,
                    TokenId = tokenDetails.TokenId,                  
                    CreatedTime = DateTime.UtcNow
                };
                genericResponse = await regManagementRepository.SaveRemoveProviderToken(removeProviderToken, TeamEnum.DSIT, EventTypeEnum.RemoveProvider, loggedInUserEmail);

                if (genericResponse.Success)
                {
                    ProviderProfile providerProfile = await regManagementRepository.GetProviderDetails(providerProfileId);

                    string reasonString = RemovalReasonsEnumExtensions.GetDescription(reason.Value);
                    await SendEmails(serviceIds, dsitUserEmails, reasonString, requstedBy, tokenDetails, providerProfile, loggedInUserEmail);
                }
            }

            return genericResponse;
        }

        private async Task SendEmails(List<int> serviceIds, List<string> dsitUserEmails, string reason, TeamEnum requstedBy, TokenDetails tokenDetails, ProviderProfile providerProfile, string loggedInUserEmail)
        {
            var filteredServiceNames = providerProfile.Services.Where(s => serviceIds.Contains(s.Id)).Select(s => s.ServiceName).ToList();
            string serviceNames = string.Join("\r", filteredServiceNames);

            if (requstedBy == TeamEnum.Provider)
            {
                string linkForEmailToProvider = configuration["DvsRegisterLink"] + "remove-provider/provider/provider-details?token=" + tokenDetails.Token;
                await emailSender.SendRequestToRemoveRecordToProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, linkForEmailToProvider);
                await emailSender.SendRequestToRemoveRecordToProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, linkForEmailToProvider);
                await emailSender.SendRecordRemovalRequestConfirmationToDSIT(providerProfile.RegisteredName, serviceNames);
            }
            else if (requstedBy == TeamEnum.DSIT)
            {
                await emailSender.RemovalRequestForApprovalToDSIT(loggedInUserEmail, serviceNames, providerProfile.RegisteredName, reason);
                string linkForEmailToDSIT = configuration["DvsRegisterLink"] + "remove-provider/dsit/provider-details?token=" + tokenDetails.Token;
                foreach (var email in dsitUserEmails)
                {
                    await emailSender.SendRemoval2iCheckToDSIT(email, email, linkForEmailToDSIT, providerProfile.RegisteredName, serviceNames, reason);
                }

            }
        }

        private ProviderStatusEnum GetProviderStatus(ICollection<Service> services, ProviderStatusEnum currentStatus)
        {
            ProviderStatusEnum providerStatus = currentStatus;
            if (services != null && services.Count > 0)
            {

                if (services.All(service => service.ServiceStatus == ServiceStatusEnum.Removed))
                {
                    providerStatus = ProviderStatusEnum.RemovedFromRegister;
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
