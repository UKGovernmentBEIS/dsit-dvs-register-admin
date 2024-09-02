﻿using AutoMapper;
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

      
      

        public async Task<CertificateInformationDto> GetCertificateInformation(int certificateInfoId)
        {
            var certificateInfo = await certificateReviewRepository.GetCertificateInformation(certificateInfoId);

            CertificateInformationDto certificateInformationDto = automapper.Map<CertificateInformationDto>(certificateInfo);
            var roles = await certificateReviewRepository.GetRoles();
            List<RoleDto> roleDtos = automapper.Map<List<RoleDto>>(roles);

            var roleIds = certificateInformationDto.CertificateInfoRoleMapping.Select(mapping => mapping.RoleId);
            certificateInformationDto.Roles = roleDtos.Where(x => roleIds.Contains(x.Id)).ToList();

            var identityProfiles = await certificateReviewRepository.GetIdentityProfiles();
            List<IdentityProfileDto> identityProfileDtos = automapper.Map<List<IdentityProfileDto>>(identityProfiles);
            var identityProfileids = certificateInformationDto.CertificateInfoIdentityProfileMapping.Select(mapping => mapping.IdentityProfileId);
            certificateInformationDto.IdentityProfiles = identityProfileDtos.Where(x => identityProfileids.Contains(x.Id)).ToList();

            var schemes = await certificateReviewRepository.GetSupplementarySchemes();
            List<SupplementarySchemeDto> supplementarySchemeDtos = automapper.Map<List<SupplementarySchemeDto>>(schemes);
            var schemeids = certificateInformationDto.CertificateInfoSupSchemeMappings?.Select(x => x.SupplementarySchemeId);
            if (schemeids!=null && schemeids.Count() > 0)
                certificateInformationDto.SupplementarySchemes = supplementarySchemeDtos.Where(x => schemeids.Contains(x.Id)).ToList();


            return certificateInformationDto;
        }

        public async Task<CertificateReviewDto> GetCertificateReview(int reviewId)
        {
            var certificateInfo = await certificateReviewRepository.GetCertificateReview(reviewId);
            CertificateReviewDto certificateReviewDto = automapper.Map<CertificateReviewDto>(certificateInfo);
            return certificateReviewDto;
        }

        public async Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons()
        {
            var rejectionReasonList = await certificateReviewRepository.GetRejectionReasons();
            return automapper.Map<List<CertificateReviewRejectionReasonDto>>(rejectionReasonList);
        }

        public async Task<CertificateInformationDto> GetProviderAndCertificateDetailsByToken(string token, string tokenId)
        {
            ConsentToken consentToken = await consentRepository.GetConsentToken(token, tokenId);
            CertificateReview certificateReview = await certificateReviewRepository.GetCertificateReview(consentToken.CertificateReviewId);
           // CertificateInformationDto certificateInformationDto = await GetCertificateInformation(certificateReview.CertificateInformationId);           
            return new CertificateInformationDto();

        }
        public async Task<GenericResponse> UpdateCertificateReviewStatus(string token, string tokenId, CertificateInformationDto certificateInformationDto)
        {
           
            GenericResponse genericResponse = new GenericResponse();
            ConsentToken consentToken = await consentRepository.GetConsentToken(token, tokenId);
            PreRegistrationDto preRegistrationDto = certificateInformationDto.Provider.PreRegistration;
            if (!string.IsNullOrEmpty(consentToken.Token)  && !string.IsNullOrEmpty(consentToken.TokenId))   //proceed update status if token exists           
            {
                var reviewEntity = await certificateReviewRepository.GetCertificateReview(consentToken.CertificateReviewId);
                if (reviewEntity != null)
                {
                    //TO DO: in register management
                    //ProviderStatusEnum providerStatus = ProviderStatusEnum.ActionRequired;
                    //List<CertificateInformation> serviceList = await certificateReviewRepository.GetCertificateInformationListByProvider(reviewEntity.ProviderId);
                  
                    //if (serviceList.Any(item => item.CertificateInfoStatus == CertificateInfoStatusEnum.Published))
                    //{
                    //    providerStatus = ProviderStatusEnum.PublishedActionRequired;
                    //}                    
                    //genericResponse =  await certificateReviewRepository.UpdateCertificateReviewStatus(consentToken.CertificateReviewId, "DIP", providerStatus);
                    //if (genericResponse.Success)
                    //{
                    //    genericResponse.Success = await emailSender.SendAgreementToPublishToDIP(preRegistrationDto?.FullName??string.Empty, preRegistrationDto?.Email??string.Empty) &&
                    //   await emailSender.SendAgreementToPublishToDSIT(preRegistrationDto?.URN??string.Empty, certificateInformationDto.ServiceName);
                    //}
                }
              
            }
            return genericResponse;
        }

        #region New methods
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
                //TODO:

                //TokenDetails tokenDetails = jwtService.GenerateToken();
                //string consentLink = configuration["ReviewPortalLink"] +"consent/give-consent?token="+tokenDetails.Token;

                ////Insert token details to db for further reference
                //ConsentToken consentToken = new ConsentToken();
                //consentToken.CertificateReviewId = genericResponse.InstanceId;
                //consentToken.Token = tokenDetails.Token;
                //consentToken.TokenId = tokenDetails.TokenId;
                //consentToken.CreatedTime = DateTime.UtcNow;
                //genericResponse = await consentRepository.SaveConsentToken(consentToken);
                if (genericResponse.Success)
                {
                    await emailSender.SendCertificateInfoApprovedToCab(serviceDto.CabUser.CabEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, serviceDto.CabUser.CabEmail);
                    await emailSender.SendCertificateInfoApprovedToDSIT(serviceDto.Provider.RegisteredName, serviceDto.ServiceName);

                    //To Do : consent email

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

        #endregion


    }
}
