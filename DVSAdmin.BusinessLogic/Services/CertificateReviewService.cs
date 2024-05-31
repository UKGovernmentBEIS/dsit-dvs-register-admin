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

            return certificateInformationDto;
        }


    }
}
