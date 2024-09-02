using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICertificateReviewService
    {
        public Task<GenericResponse> SaveCertificateReview(CertificateReviewDto cetificateReviewDto);
        public Task<GenericResponse> UpdateCertificateReview(CertificateReviewDto cetificateReviewDto, CertificateInformationDto certificateInformationDto);
        public Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReviewDto cetificateReviewDto, CertificateInformationDto certificateInformationDto, 
        List<CertificateReviewRejectionReasonDto> rejectionReasons);
        public Task<CertificateInformationDto> GetCertificateInformation(int certificateInfoId);      
        public  Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons();
        public  Task<CertificateReviewDto> GetCertificateReview(int reviewId);
        public Task<CertificateInformationDto> GetProviderAndCertificateDetailsByToken(string token, string tokenId);
        public Task<GenericResponse> UpdateCertificateReviewStatus(string token, string tokenId, CertificateInformationDto certificateInformationDto);
        #region New methods
        public Task<List<ServiceDto>> GetServiceList();
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<GenericResponse> UpdateCertificateReview(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto);
        public Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, List<CertificateReviewRejectionReasonDto> rejectionReasons);
        #endregion
    }
}
