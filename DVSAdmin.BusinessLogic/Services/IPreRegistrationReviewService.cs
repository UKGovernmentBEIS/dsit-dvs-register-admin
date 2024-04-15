using DVSAdmin.BusinessLogic.Models.PreRegistration;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPreRegistrationReviewService
    {
        public Task<List<PreRegistrationDto>> GetPreRegistrations();
    }
}
