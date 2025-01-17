using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using DVSRegister.CommonUtility.Models.Enums;
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

        public async Task<ProviderProfileDto> GetProviderDetails(int providerProfileId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerProfileId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            return providerDto;
        }

        public async Task<ProviderProfileDto> GetProviderWithServiceDeatils(int providerProfileId)
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

        /// <summary>
        /// List<int> serviceIds : all services to be passed is event type RemoveProvider,
        /// for RemoveService  selected service services to be passed
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="providerProfileId"></param>
        /// <param name="serviceIds"></param>
        /// <param name="reason"></param>
        /// <param name="loggedInUserEmail"></param>
        /// <returns></returns>
        public async Task<GenericResponse> UpdateRemovalStatus(EventTypeEnum eventType,  int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, RemovalReasonsEnum? reason, ServiceRemovalReasonEnum? serviceRemovalReason)
        {
            TeamEnum team = eventType == EventTypeEnum.RemovedByCronJob ? TeamEnum.CronJob : TeamEnum.DSIT;
            GenericResponse genericResponse = await regManagementRepository.UpdateRemovalStatus(eventType, team, providerProfileId, serviceIds, loggedInUserEmail, reason, serviceRemovalReason);

            if (genericResponse.Success)
            {

                if (eventType == EventTypeEnum.RemoveProvider || eventType == EventTypeEnum.RemoveService)
                {
                    // save token for 2i check
                    //Insert token details to db for further reference, if multiple services are removed, insert to mapping table
                    TokenDetails tokenDetails = jwtService.GenerateToken();

                    ICollection<RemoveTokenServiceMapping> removeTokenServiceMapping = [];
                    if (eventType == EventTypeEnum.RemoveService)
                    {
                        foreach (var item in serviceIds)
                        {
                            removeTokenServiceMapping.Add(new RemoveTokenServiceMapping { ServiceId = item });
                        }
                    }

                    RemoveProviderToken removeProviderToken = new()
                    {
                        ProviderProfileId = providerProfileId,
                        Token = tokenDetails.Token,
                        TokenId = tokenDetails.TokenId,
                        RemoveTokenServiceMapping = removeTokenServiceMapping,
                        CreatedTime = DateTime.UtcNow
                    };
                    genericResponse = await regManagementRepository.SaveRemoveProviderToken(removeProviderToken, TeamEnum.DSIT, eventType, loggedInUserEmail);


                    ProviderProfile providerProfile = await regManagementRepository.GetProviderDetails(providerProfileId);
                    if (reason == RemovalReasonsEnum.ProviderRequestedRemoval)
                    {
                        string linkForEmailToProvider = configuration["DvsRegisterLink"] + "remove-provider/provider/provider-details?token=" + tokenDetails.Token;
                        //To Do: email to providers
                        foreach(var email in dsitUserEmails)
                        {
                            //send email
                        }
                    }
                    else
                    {
                        string linkForEmailToDSIT = configuration["DvsRegisterLink"] + "remove-provider/dsit/provider-details?token=" + tokenDetails.Token;
                        //To do : email to dsit
                    }

                }
            }

            return genericResponse;
        }

    }
}
