using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.BusinessLogic.Services
{
    public class CertificateReviewService : ICertificateReviewService
    {
        private readonly ICertificateReviewRepository certificateReviewRepository;
        
        private readonly IMapper automapper;
        private readonly ILogger<CertificateReviewService> logger;
        private readonly IEmailSender emailSender;

        public CertificateReviewService(ICertificateReviewRepository certificateReviewRepository, IMapper automapper,
          ILogger<CertificateReviewService> logger, IEmailSender emailSender)
        {
            this.certificateReviewRepository = certificateReviewRepository;
            this.automapper = automapper;
            this.logger = logger;
            this.emailSender = emailSender;
        }


        public async Task<List<CertificateInformationDto>> GetCertificateInformationList()
        {
            var certificateInfoList = await certificateReviewRepository.GetCertificateInformationList();
            return automapper.Map<List<CertificateInformationDto>>(certificateInfoList);
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
            if(schemeids!=null && schemeids.Count() > 0)
            certificateInformationDto.SupplementarySchemes = supplementarySchemeDtos.Where(x => schemeids.Contains(x.Id)).ToList();           


            return certificateInformationDto;
        }

        public async Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons()
        {
            var rejectionReasonList = await certificateReviewRepository.GetRejectionReasons();
            return automapper.Map<List<CertificateReviewRejectionReasonDto>>(rejectionReasonList);
        }


    }
}
