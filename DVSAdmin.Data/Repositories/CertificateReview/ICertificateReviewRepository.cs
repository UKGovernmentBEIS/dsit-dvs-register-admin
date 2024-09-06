using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface ICertificateReviewRepository
    {
        public Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview);
        public Task<GenericResponse> UpdateCertificateReview(CertificateReview cetificateReview);
        public Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReview cetificateReview);        
        public Task<List<Role>> GetRoles();
        public Task<List<IdentityProfile>> GetIdentityProfiles();
        public Task<List<SupplementaryScheme>> GetSupplementarySchemes();
        public Task<List<CertificateInformation>> GetCertificateInformationList();
        public Task<List<Service>> GetServiceListByProvider(int providerId);        
        public Task<List<CertificateReviewRejectionReason>> GetRejectionReasons();
        public Task<CertificateReview> GetCertificateReview(int reviewId);
        public Task<GenericResponse> UpdateCertificateReviewStatus(int certificateReviewId, string modifiedBy, ProviderStatusEnum providerStatus);
        #region New path
        public Task<List<Service>> GetServiceList();
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<GenericResponse> UpdateServiceStatus(int serviceId, ServiceStatusEnum serviceStatus);
        #endregion
    }
}
