using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface ICertificateReviewRepository
    {
        public Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview);
        public Task<CertificateInformation> GetCertificateInformation(int certificateInfoId);
        public Task<List<Role>> GetRoles();
        public Task<List<IdentityProfile>> GetIdentityProfiles();
        public Task<List<SupplementaryScheme>> GetSupplementarySchemes();
        public Task<List<CertificateInformation>> GetCertificateInformationList();
    }
}
