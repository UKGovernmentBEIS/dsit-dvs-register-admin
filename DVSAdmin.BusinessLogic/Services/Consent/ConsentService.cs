﻿using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class ConsentService : IConsentService
    {

        private readonly IConsentRepository consentRepository;


        public ConsentService(IConsentRepository consentRepository)
        {           
            this.consentRepository = consentRepository;            
        }

        public async Task<bool> RemoveConsentToken(string token, string tokenId)
        {
            return await consentRepository.RemoveConsentToken(token, tokenId);
        }


        public async Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId)
        {
            return await consentRepository.RemoveProceedApplicationConsentToken(token, tokenId);
        }

    }
}
