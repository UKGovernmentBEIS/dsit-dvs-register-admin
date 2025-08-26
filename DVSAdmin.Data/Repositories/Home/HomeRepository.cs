using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace DVSAdmin.Data.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly DVSAdminDbContext _context;
        private readonly ILogger<HomeRepository> _logger;
        public HomeRepository(DVSAdminDbContext context, ILogger<HomeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<PaginatedResult<Service>> GetPendingCertificateReviews(int pageNumber, string sort, string sortAction)
        {
            try
            {
                var baseQuery = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Where(s => ((s.ServiceStatus == ServiceStatusEnum.Submitted || s.ServiceStatus == ServiceStatusEnum.Resubmitted) &&
                s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.Id != s.CertificateReview.ServiceId) ||
                (s.CertificateReview != null &&
                (s.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.DeclinedByProvider)));

                return await SortAndPaginate(pageNumber, sort, sortAction, baseQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
               
            }
        
        }

      

        public async Task<PaginatedResult<Service>> GetPendingPrimaryChecks(string loggedInUserEmail,int pageNumber, string sort, string sortAction)
        {
            try
            {
                User loggedInUser = await _context.User.Where(x => x.Email == loggedInUserEmail).FirstOrDefaultAsync();
                var baseQuery = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.PublicInterestCheck)
                .Where(s => (s.ServiceStatus == ServiceStatusEnum.Received &&
                s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.Id != s.PublicInterestCheck.ServiceId) ||
                (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer
                && s.PublicInterestCheck.SecondaryCheckUserId != loggedInUser.Id));
                return await SortAndPaginate(pageNumber, sort, sortAction, baseQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }

        }

        public async Task<PaginatedResult<Service>> GetPendingSecondaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction)
        {
            try
            {
                User loggedInUser = await _context.User.Where(x => x.Email == loggedInUserEmail).FirstOrDefaultAsync();
                var baseQuery = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.PublicInterestCheck)
                .Where(s => (s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.PublicInterestCheck != null &&
                (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
                s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed) && s.PublicInterestCheck.PrimaryCheckUserId != loggedInUser.Id));


                return await SortAndPaginate(pageNumber, sort, sortAction, baseQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }

        }
        public async Task<PaginatedResult<Service>> GetPendingRequests(string loggedInUserEmail, int pageNumber, string sort, string sortAction)
        {
            try
            {
                User loggedInUser = await _context.User.Where(x => x.Email == loggedInUserEmail).FirstOrDefaultAsync();
                var baseQuery = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.ServiceDraft)
                .Where(s => (s.ServiceStatus == ServiceStatusEnum.UpdatesRequested && s.ServiceDraft.RequestedUserId != loggedInUser.Id)
                         || (s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && s.RemovalRequestedUser != loggedInUser.Id && s.ServiceRemovalReason != ServiceRemovalReasonEnum.ProviderRequestedRemoval)
                         || (s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && s.RemovalRequestedUser != loggedInUser.Id && s.ServiceRemovalReason == null && s.Provider.RemovalReason != RemovalReasonsEnum.ProviderRequestedRemoval)
                         || s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation);


                return await SortAndPaginate(pageNumber, sort, sortAction, baseQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }

        }



        public async Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail)
        {
            User loggedInUser = await _context.User.Where(x => x.Email == loggedInUserEmail).FirstOrDefaultAsync();          

            int certificateReviewCount = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Where(s => ((s.ServiceStatus == ServiceStatusEnum.Submitted || s.ServiceStatus == ServiceStatusEnum.Resubmitted) &&
                s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.Id != s.CertificateReview.ServiceId) ||
                (s.CertificateReview != null &&
                (s.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.DeclinedByProvider))).Count();
                
            int primaryCount = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.PublicInterestCheck)
                .Where(s => (s.ServiceStatus == ServiceStatusEnum.Received &&
                s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.Id != s.PublicInterestCheck.ServiceId) ||
                (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer
                && s.PublicInterestCheck.SecondaryCheckUserId != loggedInUser.Id)).Count();

            int secondaryCount = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.PublicInterestCheck)
                .Where(s => (s.ServiceStatus != ServiceStatusEnum.Removed &&
                s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                s.PublicInterestCheck != null &&
                (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
                s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed) && s.PublicInterestCheck.PrimaryCheckUserId != loggedInUser.Id)).Count(); ;
            int updateOrRemovalCount = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.ServiceDraft)
                .Where(s => (s.ServiceStatus == ServiceStatusEnum.UpdatesRequested && s.ServiceDraft.RequestedUserId != loggedInUser.Id)
                         || (s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && s.RemovalRequestedUser != loggedInUser.Id && s.ServiceRemovalReason != ServiceRemovalReasonEnum.ProviderRequestedRemoval)
                         || (s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && s.RemovalRequestedUser != loggedInUser.Id && s.ServiceRemovalReason == null && s.Provider.RemovalReason != RemovalReasonsEnum.ProviderRequestedRemoval)
                         || s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation).Count();
           

            return new Dictionary<string, int>
            {
                ["CertificateReview"] = certificateReviewCount,
                ["Primary"] = primaryCount,
                ["Secondary"] = secondaryCount,
                ["UpdateOrRemoval"] = updateOrRemovalCount
            };
        }


        public async Task<User> GetUserByEmail(string userEmail)
        {
            return await _context.User.FirstOrDefaultAsync<User>(e => e.Email == userEmail);
        }

        #region Private methods
        private static async Task<PaginatedResult<Service>> SortAndPaginate(int pageNumber, string sort, string sortAction, IQueryable<Service> baseQuery)
        {
            var sortedResults = sort switch
            {
                "service name" => sortAction == "descending"
                    ? baseQuery.OrderByDescending(r => r.ServiceName)
                    : baseQuery.OrderBy(r => r.ServiceName),

                "provider" => sortAction == "descending"
                    ? baseQuery.OrderByDescending(r => r.Provider.RegisteredName)
                    : baseQuery.OrderBy(r => r.Provider.RegisteredName),

                "days" => sortAction == "descending"
                ? baseQuery.OrderByDescending(r => (r.ModifiedTime ?? r.CreatedTime))
                : baseQuery.OrderBy(r => (r.ModifiedTime ?? r.CreatedTime)),


                "application" => sortAction == "descending"
                    ? baseQuery.OrderByDescending(r => r.ServiceVersion)
                    : baseQuery.OrderBy(r => r.ServiceVersion),
                "status" => sortAction == "descending"
                   ? baseQuery.OrderByDescending(r => r.ServiceStatus)
                   : baseQuery.OrderBy(r => r.ServiceStatus),
                _ => sortAction == "descending"
                    ? baseQuery.OrderByDescending(r => r.ModifiedTime)
                    : baseQuery.OrderBy(r => r.ModifiedTime),
            };


            var paged = sortedResults
               .Skip((pageNumber - 1) * 10)
               .Take(10)
               .ToListAsync();

            return new PaginatedResult<Service>
            {
                Items = await paged,
                TotalCount = sortedResults.ToList().Count
            };
        }
        #endregion
    }
}
