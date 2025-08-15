using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface ICertificateReviewRepository
    {
        
        public Task<List<Role>> GetRoles();
        public Task<List<IdentityProfile>> GetIdentityProfiles();
        public Task<List<SupplementaryScheme>> GetSupplementarySchemes();     
        
        public Task<List<CertificateReviewRejectionReason>> GetRejectionReasons();
        public Task<CertificateReview> GetCertificateReview(int reviewId);
        public Task<CertificateReview> GetCertificateReviewWithRejectionData(int reviewId);
        public Task<List<Service>> GetServiceList(string searchText = "");     
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<Service> GetPreviousServiceVersion(int currentServiceId);

        #region Save - update 

        public Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview, string loggedInUserEmail);
        public Task<GenericResponse> UpdateCertificateReview(CertificateReview cetificateReview, string loggedInUserEmail);
        public Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReview cetificateReview, string loggedInUserEmail);
        public Task<GenericResponse> UpdateCertificateSentBack(CertificateReview cetificateReview, string loggedInUserEmail);
        public Task<GenericResponse> RestoreRejectedCertificateReview(int serviceId, string loggedInUserEmail);
        #endregion
    }
}
