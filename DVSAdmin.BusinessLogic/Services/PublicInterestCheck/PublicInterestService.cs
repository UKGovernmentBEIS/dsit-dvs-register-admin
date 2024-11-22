using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
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
        private readonly IEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;


        public PublicInterestService(IPublicInterestCheckRepository publicInterestCheckRepository, IMapper automapper,
          IEmailSender emailSender, IConsentRepository consentRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.publicInterestCheckRepository = publicInterestCheckRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.consentRepository = consentRepository;
            this.jwtService = jwtService;
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

        public async Task<ServiceDto> GetServiceDetailsWithMappings(int serviceId)
        {
            var certificateInfo = await publicInterestCheckRepository.GetServiceDetailsWithMappings(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(certificateInfo);
            return serviceDto;
        }
        public async Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType, string loggedInUserEmail)
        {
            Service service = await publicInterestCheckRepository.GetServiceDetails(publicInterestCheckDto.ServiceId);

            PublicInterestCheck publicInterestCheck = new();
            automapper.Map(publicInterestCheckDto, publicInterestCheck);
            GenericResponse genericResponse = await publicInterestCheckRepository.SavePublicInterestCheck(publicInterestCheck, reviewType, loggedInUserEmail);
           
            if (genericResponse.Success)
            {
                PICheckLogs pICheckLog = new();
                pICheckLog.PublicInterestCheckId = genericResponse.InstanceId;
                pICheckLog.LogTime = DateTime.UtcNow;
                if (reviewType == ReviewTypeEnum.PrimaryCheck)
                {
                    pICheckLog.ReviewType = ReviewTypeEnum.PrimaryCheck;
                    pICheckLog.Comment = publicInterestCheckDto.PrimaryCheckComment;
                    pICheckLog.UserId = publicInterestCheckDto.PrimaryCheckUserId;
                }
                else
                {
                    pICheckLog.ReviewType = ReviewTypeEnum.SecondaryCheck;
                    pICheckLog.Comment = publicInterestCheckDto.SecondaryCheckComment;
                    pICheckLog.UserId = Convert.ToInt32(publicInterestCheckDto.SecondaryCheckUserId);
                }

                await publicInterestCheckRepository.SavePICheckLog(pICheckLog, loggedInUserEmail);


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
                        await emailSender.SendApplicationRejectedToDIP(service.Provider.PrimaryContactFullName, service.Provider.PrimaryContactEmail);
                        await emailSender.SendApplicationRejectedToDIP(service.Provider.SecondaryContactFullName, service.Provider.SecondaryContactEmail);
                        await emailSender.SendApplicationRejectedConfirmationToDSIT(service.Provider.RegisteredName, service.ServiceName);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckPassed)
                    {


                        TokenDetails tokenDetails = jwtService.GenerateToken();
                        string consentLink = configuration["DvsRegisterLink"] +"consent/publish-service-give-consent?token="+tokenDetails.Token;

                        //Insert token details to db for further reference
                        ProceedPublishConsentToken consentToken = new ();
                        consentToken.ServiceId = publicInterestCheckDto.ServiceId;
                        consentToken.Token = tokenDetails.Token;
                        consentToken.TokenId = tokenDetails.TokenId;
                        consentToken.CreatedTime = DateTime.UtcNow;
                        genericResponse = await consentRepository.SaveConsentToken(consentToken, loggedInUserEmail);


                        await emailSender.SendApplicationApprovedToDSIT(service.Provider.RegisteredName, service.ServiceName);

                        await emailSender.SendConsentToPublishToDIP(service.Provider.RegisteredName, service.ServiceName,
                        service.Provider.PrimaryContactFullName,consentLink, service.Provider.PrimaryContactEmail);

                        await emailSender.SendConsentToPublishToDIP(service.Provider.RegisteredName, service.ServiceName,
                        service.Provider.SecondaryContactFullName, consentLink, service.Provider.SecondaryContactEmail);


                    }
                }
            }
            return genericResponse;
        }

        public async Task<ServiceDto> GetProviderAndCertificateDetailsByConsentToken(string token, string tokenId)
        {
            ProceedPublishConsentToken consentToken = await consentRepository.GetConsentToken(token, tokenId);
            var serviceDto = await GetServiceDetailsWithMappings(consentToken.ServiceId);
            return serviceDto;
        }


     
    }
}