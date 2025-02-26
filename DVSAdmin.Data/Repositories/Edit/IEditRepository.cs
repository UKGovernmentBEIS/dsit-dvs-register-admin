using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IEditRepository
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraft draft, string loggedInUserEmail);
        public Task<Service> GetService(int serviceId);

    }
}
