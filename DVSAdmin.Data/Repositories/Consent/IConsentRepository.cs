using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IConsentRepository
    {
        public Task<GenericResponse> SaveConsentToken(ConsentToken consentToken);
        public Task<bool> RemoveConsentToken(string token, string tokenId);
        public Task<ConsentToken> GetConsentToken(string token, string tokenId);
    }
}
