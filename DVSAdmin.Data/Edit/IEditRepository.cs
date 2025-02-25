using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Edit
{
    public interface IEditRepository
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraft draft, string loggedInUserEmail);
        
    }
}
