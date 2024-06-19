using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IPreRegistrationReviewRepository
    {
        public Task<List<PreRegistration>> GetPreRegistrations();
        public Task<PreRegistration> GetPreRegistration(int preRegistrationId);
        public Task<GenericResponse> SavePreRegistrationReview(PreRegistrationReview preRegistrationReview, ReviewTypeEnum reviewType);
        public Task<List<Country>> GetCountries();
        public Task<GenericResponse> UpdateURNStatus(UniqueReferenceNumber uniqueReferenceNumber);
    }
}
