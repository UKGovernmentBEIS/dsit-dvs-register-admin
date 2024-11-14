namespace DVSAdmin.BusinessLogic.Services
{
    public interface IConsentService
    {
        public Task<bool> RemoveConsentToken(string token, string tokenId, string loggedInUserEmail);
        public Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId, string loggedInUserEmail);
    }
}
