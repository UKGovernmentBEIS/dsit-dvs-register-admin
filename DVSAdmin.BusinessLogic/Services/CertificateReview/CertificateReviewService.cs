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
    public class CertificateReviewService : ICertificateReviewService
    {
        private readonly ICertificateReviewRepository certificateReviewRepository;
        private readonly IConsentRepository consentRepository;
        private readonly IMapper automapper;       
        private readonly CertificateReviewEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;

        public CertificateReviewService(ICertificateReviewRepository certificateReviewRepository, IMapper automapper,
          CertificateReviewEmailSender emailSender, IConsentRepository consentRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.certificateReviewRepository = certificateReviewRepository;
            this.automapper = automapper;            
            this.emailSender = emailSender;
            this.consentRepository=consentRepository;
            this.jwtService = jwtService;
            this.configuration = configuration;
        }
           

        public async Task<GenericResponse> SaveCertificateReview(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, string loggedInUserEmail,List<CertificateReviewRejectionReasonDto> rejectionReasons = null!)
        {
            CertificateReview certificateReview = new();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.SaveCertificateReview(certificateReview, loggedInUserEmail);
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Approved)
            {

                genericResponse = await GenerateTokenAndSendEmail(serviceDto, loggedInUserEmail, false);
                if (genericResponse.Success)
                {
                    await emailSender.SendCertificateInfoApprovedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail);
                    await emailSender.SendCertificateInfoApprovedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName);

                }
            }
            else if(genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Rejected)
            {
                string rejectReasons = string.Join("\r", rejectionReasons.Select(m => m.Reason));
                await emailSender.SendCertificateInfoRejectedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, cetificateReviewDto.RejectionComments, serviceDto.CabUser.CabEmail);
                await emailSender.SendCertificateInfoRejectedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, certificateReview.RejectionComments);
            }
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)
            {
                await emailSender.SendCertificateBackToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail, certificateReview.Amendments);
                await emailSender.SendCertificateBackDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName, certificateReview.Amendments);

            }
            return genericResponse;

        }    

        public async Task<CertificateReviewDto> GetCertificateReview(int reviewId)
        {
            var certificateInfo = await certificateReviewRepository.GetCertificateReview(reviewId);
            CertificateReviewDto certificateReviewDto = automapper.Map<CertificateReviewDto>(certificateInfo);
            return certificateReviewDto;
        }

        public async Task<CertificateReviewDto> GetCertificateReviewWithRejectionData(int reviewId)
        {
            var certificateInfo = await certificateReviewRepository.GetCertificateReviewWithRejectionData(reviewId);
            CertificateReviewDto certificateReviewDto = automapper.Map<CertificateReviewDto>(certificateInfo);
            return certificateReviewDto;
        }


        public async Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons()
        {
            var rejectionReasonList = await certificateReviewRepository.GetRejectionReasons();
            return automapper.Map<List<CertificateReviewRejectionReasonDto>>(rejectionReasonList);
        }

        public async Task<List<ServiceDto>> GetServiceList(string searchText = "")
        {
            var serviceList = await certificateReviewRepository.GetServiceList(searchText);
            return automapper.Map<List<ServiceDto>>(serviceList);
        }

      

        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var certificateInfo = await certificateReviewRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(certificateInfo);
            return serviceDto;
        }

        public async Task<ServiceDto> GetPreviousServiceVersion(int currentServiceId)
        {
            var certificateInfo = await certificateReviewRepository.GetPreviousServiceVersion(currentServiceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(certificateInfo);
            return serviceDto;
        }





        public async Task<GenericResponse> UpdateCertificateReview(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, string loggedInUserEmail)
        {
            CertificateReview certificateReview = new();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.UpdateCertificateReview(certificateReview, loggedInUserEmail);
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Approved)
            {

                genericResponse = await GenerateTokenAndSendEmail(serviceDto, loggedInUserEmail, false);
                if (genericResponse.Success)
                {
                    await emailSender.SendCertificateInfoApprovedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail);
                    await emailSender.SendCertificateInfoApprovedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName);                  
                   
                }
            }
            return genericResponse;
        }


        public async Task<GenericResponse> GenerateTokenAndSendEmail(ServiceDto serviceDto, string loggedInUserEmail, bool isResend)
        {
            TokenDetails tokenDetails = jwtService.GenerateToken(string.Empty, serviceDto.ProviderProfileId, serviceDto.Id.ToString());
            string consentLink = configuration["DvsRegisterLink"] + "consent/proceed-application-consent?token=" + tokenDetails.Token;

            //Insert token details to db for authorization on clicking opening loop link
            ProceedApplicationConsentToken consentToken = new()
            {
                ServiceId = serviceDto.Id,
                Token = tokenDetails.Token,
                TokenId = tokenDetails.TokenId          
            };
            GenericResponse genericResponse = await consentRepository.SaveProceedApplicationConsentToken(consentToken, loggedInUserEmail);
            if(genericResponse.Success)  
            {
                List<string> emailList = [serviceDto.Provider.PrimaryContactEmail, serviceDto.Provider.SecondaryContactEmail];
                await emailSender.SendProceedApplicationConsentToDIP(serviceDto.ServiceName, serviceDto.Provider.PrimaryContactFullName, 
                    serviceDto.Provider.SecondaryContactFullName, consentLink, emailList);

                if(isResend) 
                {
                    await emailSender.ConfirmationConsentResentToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName);
                }
            }
            return genericResponse;

        }

        public async Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, List<CertificateReviewRejectionReasonDto> rejectionReasons, string loggedInUserEmail)
        {
            CertificateReview certificateReview = new CertificateReview();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.UpdateCertificateReviewRejection(certificateReview, loggedInUserEmail);

       
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Rejected )
            {
                string rejectReasons = string.Join("\r", rejectionReasons.Select(m => m.Reason));
                await emailSender.SendCertificateInfoRejectedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, cetificateReviewDto.RejectionComments, serviceDto.CabUser.CabEmail);
                await emailSender.SendCertificateInfoRejectedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, certificateReview.RejectionComments);

            }

            return genericResponse;
        }

        public async Task<GenericResponse> UpdateCertificateSentBack(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, string loggedInUserEmail)
        {
            CertificateReview certificateReview = new CertificateReview();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.UpdateCertificateSentBack(certificateReview, loggedInUserEmail);


            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)
            {
                await emailSender.SendCertificateBackToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail, certificateReview.Amendments);
                await emailSender.SendCertificateBackDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName, certificateReview.Amendments);

            }

            return genericResponse;
        }


        public async Task<GenericResponse> RestoreRejectedCertificateReview(int reviewId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await certificateReviewRepository.RestoreRejectedCertificateReview(reviewId, loggedInUserEmail);
            if (genericResponse.Success)
            {
                await emailSender.SendApplicationRestroredToDSIT();
            }         
            return genericResponse;
        }

    }
}
