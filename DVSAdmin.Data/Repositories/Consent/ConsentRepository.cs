﻿using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories
{
    public class ConsentRepository : IConsentRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<ConsentRepository> logger;

        public ConsentRepository (DVSAdminDbContext context, ILogger<ConsentRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public async Task<GenericResponse> SaveConsentToken(ConsentToken consentToken)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.ConsentToken.FirstOrDefaultAsync(e => e.Token == consentToken.Token && e.TokenId == consentToken.TokenId);

                if (existingEntity == null)
                {
                    await context.ConsentToken.AddAsync(consentToken);
                    await context.SaveChangesAsync();
                    transaction.Commit();
                    genericResponse.Success = true;
                }

            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError(ex.Message);
            }
            return genericResponse;
        }

        public async Task<ConsentToken> GetConsentToken(string token, string tokenId)
        {
            return await context.ConsentToken.FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId)??new ConsentToken();
        }

        public async Task<bool> RemoveConsentToken(string token, string tokenId)
        {
            var consent = await context.ConsentToken.FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId);

            if (consent != null)
            {
                context.ConsentToken.Remove(consent);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
