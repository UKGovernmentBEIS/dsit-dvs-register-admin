using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IConsentService
    {
        public Task<bool> RemoveConsentToken(string token, string tokenId);
        public Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId);
    }
}
