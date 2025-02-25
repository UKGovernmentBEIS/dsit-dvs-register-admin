using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.BusinessLogic.Models.Draft;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IEditService
    {
        Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail);
        Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail);
    }
}
