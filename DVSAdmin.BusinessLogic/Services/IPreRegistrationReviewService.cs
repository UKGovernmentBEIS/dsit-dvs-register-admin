using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPreRegistrationReviewService
    {
        public Task<List<PreRegistrationDto>> GetPreRegistrations();

        public Task<PreRegistrationDto> GetPreRegistration(int preRegistrationId);
        public Task<GenericResponse> SavePreRegistrationReview(PreRegistrationReviewDto preRegistrationReviewDto, ReviewTypeEnum reviewType);
    }
}
