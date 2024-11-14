using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class ConsentService : IConsentService
    {

        private readonly IConsentRepository consentRepository;


        public ConsentService(IConsentRepository consentRepository)
        {           
            this.consentRepository = consentRepository;            
        }

        public async Task<bool> RemoveConsentToken(string token, string tokenId,string loggedInUserEmail)
        {
            return await consentRepository.RemoveConsentToken(token, tokenId, loggedInUserEmail);
        }


        public async Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId, string loggedInUserEmail)
        {
            return await consentRepository.RemoveProceedApplicationConsentToken(token, tokenId, loggedInUserEmail);
        }

    }
}
