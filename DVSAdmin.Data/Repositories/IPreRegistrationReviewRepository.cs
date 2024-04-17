using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IPreRegistrationReviewRepository
    {
        public Task<List<PreRegistration>> GetPreRegistrations();
    }
}
