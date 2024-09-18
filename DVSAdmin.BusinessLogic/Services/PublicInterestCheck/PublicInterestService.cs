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
        public async Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType)
        {
            Service service = await publicInterestCheckRepository.GetServiceDetails(publicInterestCheckDto.ServiceId);

            PublicInterestCheck publicInterestCheck = new();
            automapper.Map(publicInterestCheckDto, publicInterestCheck);
            GenericResponse genericResponse = await publicInterestCheckRepository.SavePublicInterestCheck(publicInterestCheck, reviewType);

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
                       // await emailSender.SendPrimaryCheckRoundTwoConfirmationToOfDia(preRegistration.URN??string.Empty, expirationDate);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckFailed)
                    {

                        // await emailSender.SendPrimaryApplicationRejectedConfirmationToOfDia(preRegistration.URN??string.Empty);
                        //await emailSender.SendApplicationRejectedToDIASP(preRegistration.FullName, preRegistration.Email);
                        //await emailSender.SendApplicationRejectedToDIASP(preRegistration.SponsorFullName??string.Empty, preRegistration.SponsorEmail);
                    }
                    else if (publicInterestCheckDto.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckPassed)
                    {

                        // await emailSender.SendURNIssuedConfirmationToOfDia(preRegistration.URN??string.Empty); //email to ofdia                       
                        //await emailSender.SendApplicationApprovedToDIASP(preRegistration.FullName, preRegistration.URN??string.Empty, //email to provider
                        //await emailSender.SendApplicationApprovedToDIASP(preRegistration.SponsorFullName??string.Empty, preRegistration.URN??string.Empty,
                    }
                }
            }
            return genericResponse;
        }
    }
}