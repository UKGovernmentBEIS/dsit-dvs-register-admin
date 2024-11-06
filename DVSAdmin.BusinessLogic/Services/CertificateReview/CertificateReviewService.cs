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
        private readonly IEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;

        public CertificateReviewService(ICertificateReviewRepository certificateReviewRepository, IMapper automapper,
          IEmailSender emailSender, IConsentRepository consentRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.certificateReviewRepository = certificateReviewRepository;
            this.automapper = automapper;            
            this.emailSender = emailSender;
            this.consentRepository=consentRepository;
            this.jwtService = jwtService;
            this.configuration = configuration;
        }
           

        public async Task<GenericResponse> SaveCertificateReview(CertificateReviewDto cetificateReviewDto)
        {
            CertificateReview certificateReview = new CertificateReview();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.SaveCertificateReview(certificateReview);
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
        public async Task<List<ServiceDto>> GetServiceList()
        {
            var serviceList = await certificateReviewRepository.GetServiceList();
            return automapper.Map<List<ServiceDto>>(serviceList);
        }
        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var certificateInfo = await certificateReviewRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(certificateInfo);
            return serviceDto;
        }

        public async Task<GenericResponse> UpdateCertificateReview(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto)
        {
            CertificateReview certificateReview = new CertificateReview();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.UpdateCertificateReview(certificateReview);
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Approved)
            {
              
                TokenDetails tokenDetails = jwtService.GenerateToken();
                string consentLink = configuration["ReviewPortalLink"] +"consent/proceed-application-consent?token="+tokenDetails.Token;

                //Insert token details to db for further reference
                ProceedApplicationConsentToken consentToken = new ProceedApplicationConsentToken();
                consentToken.ServiceId = serviceDto.Id;
                consentToken.Token = tokenDetails.Token;
                consentToken.TokenId = tokenDetails.TokenId;
                consentToken.CreatedTime = DateTime.UtcNow;
                genericResponse = await consentRepository.SaveProceedApplicationConsentToken(consentToken);
                if (genericResponse.Success)
                {
                    await emailSender.SendCertificateInfoApprovedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail);
                    await emailSender.SendCertificateInfoApprovedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName);
                    //Send Email to primary contact and secondary contact
                    List<string> emailList = [serviceDto.Provider.PrimaryContactEmail, serviceDto.Provider.SecondaryContactEmail];
                    await emailSender.SendProceedApplicationConsentToDIP(serviceDto.Provider.RegisteredName, serviceDto.ServiceName,
                    !string.IsNullOrEmpty(serviceDto.Provider.CompanyRegistrationNumber) ? serviceDto.Provider.CompanyRegistrationNumber : serviceDto.Provider.DUNSNumber??string.Empty,
                    serviceDto.CompanyAddress, serviceDto.Provider.PublicContactEmail, serviceDto.Provider.ProviderTelephoneNumber??string.Empty, consentLink, emailList);
                }

            }
            return genericResponse;
        }


        public async Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, List<CertificateReviewRejectionReasonDto> rejectionReasons)
        {
            CertificateReview certificateReview = new CertificateReview();
            automapper.Map(cetificateReviewDto, certificateReview);
            GenericResponse genericResponse = await certificateReviewRepository.UpdateCertificateReviewRejection(certificateReview);

       
            if (genericResponse.Success && cetificateReviewDto.CertificateReviewStatus == CertificateReviewEnum.Rejected )
            {
                string rejectReasons = string.Join("\r", rejectionReasons.Select(m => m.Reason));
                await emailSender.SendCertificateInfoRejectedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, cetificateReviewDto.RejectionComments, serviceDto.CabUser.CabEmail);
                await emailSender.SendCertificateInfoRejectedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName, rejectReasons, certificateReview.RejectionComments);

            }

            return genericResponse;
        }

        public async Task<ServiceDto> GetProviderAndCertificateDetailsByToken(string token, string tokenId)
        {
            ProceedApplicationConsentToken consentToken = await consentRepository.GetProceedApplicationConsentToken(token, tokenId);          
            var serviceDto = await GetServiceDetails(consentToken.ServiceId);
            return serviceDto;
        }

        public async Task<GenericResponse> UpdateServiceStatus(int serviceId)
        {
            GenericResponse genericResponse = await certificateReviewRepository.UpdateServiceStatus(serviceId, ServiceStatusEnum.Received);
            return genericResponse;
        }

        public async Task<GenericResponse> RestoreRejectedCertificateReview(int reviewId)
        {
            GenericResponse genericResponse = await certificateReviewRepository.RestoreRejectedCertificateReview(reviewId);
            if (genericResponse.Success)
            {
                await emailSender.SendApplicationRestroredToDSIT();
            }         
            return genericResponse;
        }

    }
}
