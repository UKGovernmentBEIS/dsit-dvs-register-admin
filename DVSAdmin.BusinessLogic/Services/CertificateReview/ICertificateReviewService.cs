using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICertificateReviewService
    {
       
        public  Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons();
        public  Task<CertificateReviewDto> GetCertificateReview(int reviewId);
        public Task<CertificateReviewDto> GetCertificateReviewWithRejectionData(int reviewId);
        public Task<List<ServiceDto>> GetServiceList();
        public Task<ServiceDto> GetServiceDetails(int serviceId);     


        #region save update methods
        public Task<GenericResponse> SaveCertificateReview(CertificateReviewDto cetificateReviewDto, string loggedInUserEmail);
        public Task<GenericResponse> UpdateCertificateReview(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, string loggedInUserEmail);
        public Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReviewDto cetificateReviewDto, ServiceDto serviceDto, List<CertificateReviewRejectionReasonDto> rejectionReasons, string loggedInUserEmail);      
        public Task<GenericResponse> RestoreRejectedCertificateReview(int reviewId, string loggedInUserEmail);
        #endregion
    }
}
