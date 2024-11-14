using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IConsentRepository
    {
        #region opening the loop
        public Task<ProceedApplicationConsentToken> GetProceedApplicationConsentToken(string token, string tokenId);
        public Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId, string loggedInUserEmail);
        public Task<GenericResponse> SaveProceedApplicationConsentToken(ProceedApplicationConsentToken consentToken, string loggedInUserEmail);
        #endregion


        #region closing the loop
        public Task<GenericResponse> SaveConsentToken(ProceedPublishConsentToken consentToken);
        public Task<bool> RemoveConsentToken(string token, string tokenId);
        public Task<ProceedPublishConsentToken> GetConsentToken(string token, string tokenId);
        #endregion
    }
}
