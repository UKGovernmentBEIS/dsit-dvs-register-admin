using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
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




        #region Opening Loop

        public async Task<GenericResponse> SaveProceedApplicationConsentToken(ProceedApplicationConsentToken consentToken, string loggedinUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.ProceedApplicationConsentToken.FirstOrDefaultAsync(e => e.ServiceId == consentToken.ServiceId);
                var service = await context.Service.FirstOrDefaultAsync(s => s.Id == consentToken.ServiceId);
                service.OpeningLoopTokenStatus = TokenStatusEnum.Requested;// update token status
                if (existingEntity == null)
                {
                    consentToken.CreatedTime = DateTime.UtcNow;
                    await context.ProceedApplicationConsentToken.AddAsync(consentToken);
                    
                }
                else
                {
                    existingEntity.Token = existingEntity.Token;
                    existingEntity.TokenId = existingEntity.TokenId;                   
                    existingEntity.ModifiedTime = existingEntity.ModifiedTime;
                }
                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.AddOpeningLoopToken, loggedinUserEmail);
                transaction.Commit();
                genericResponse.Success = true;

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

        public async Task<ProceedApplicationConsentToken> GetProceedApplicationConsentToken(string token, string tokenId)
        {
            return await context.ProceedApplicationConsentToken.Include(p => p.Service).ThenInclude(p => p.Provider)
            .FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId)??new ProceedApplicationConsentToken();
        }

        public async Task<bool> RemoveProceedApplicationConsentToken(string token, string tokenId, string loggedinUserEmail)
        {
            var consent = await context.ProceedApplicationConsentToken.FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId);

            if (consent != null)
            {
                context.ProceedApplicationConsentToken.Remove(consent);
                await context.SaveChangesAsync(TeamEnum.Provider, EventTypeEnum.RemoveOpeningLoopToken, loggedinUserEmail);
                logger.LogInformation("Opening Loop : Token Removed for service {0}", consent.Service.ServiceName);
                return true;
            }

            return false;
        }
        #endregion


        #region closing the loop

        public async Task<GenericResponse> SaveConsentToken(ProceedPublishConsentToken consentToken, string loggedinUserEmail)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.ProceedPublishConsentToken.FirstOrDefaultAsync(e => e.Token == consentToken.Token && e.TokenId == consentToken.TokenId);

                if (existingEntity == null)
                {
                    await context.ProceedPublishConsentToken.AddAsync(consentToken);
                    await context.SaveChangesAsync(TeamEnum.Provider, EventTypeEnum.AddClosingLoopToken, loggedinUserEmail);
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

        public async Task<ProceedPublishConsentToken> GetConsentToken(string token, string tokenId)
        {
            return await context.ProceedPublishConsentToken.FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId)??new ProceedPublishConsentToken();
        }
     
        public async Task<bool> RemoveConsentToken(string token, string tokenId, string loggedInUserEmail)
        {
            var consent = await context.ProceedPublishConsentToken.Include(p => p.Service)
           .FirstOrDefaultAsync(e => e.Token == token && e.TokenId == tokenId);
            if (consent != null)
            {
                context.ProceedPublishConsentToken.Remove(consent);
                await context.SaveChangesAsync(TeamEnum.Provider, EventTypeEnum.RemoveClosingLoopToken, loggedInUserEmail);
                logger.LogInformation("Closing Loop : Token Removed for service {0}", consent.Service.ServiceName);
                return true;                
            }

            return false;
        }
        #endregion

    }
}
