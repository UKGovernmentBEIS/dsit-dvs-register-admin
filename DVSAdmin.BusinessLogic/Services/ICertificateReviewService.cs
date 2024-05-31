using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICertificateReviewService
    {
        public Task<GenericResponse> SaveCertificateReview(CertificateReviewDto cetificateReviewDto);
        public Task<CertificateInformationDto> GetCertificateInformation(int certificateInfoId);
        public Task<List<CertificateInformationDto>> GetCertificateInformationList();
        public  Task<List<CertificateReviewRejectionReasonDto>> GetRejectionReasons();
    }
}
