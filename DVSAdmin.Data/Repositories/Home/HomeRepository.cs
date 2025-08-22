using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static DVSAdmin.Data.Repositories.CabTransferRepository;
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

        public async Task<PaginatedResult<Service>> GetServices(int pageNumber, string sort, string sortAction, string openTask)
        {
            var baseQuery = _context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Include(s => s.PublicInterestCheck);

            var groupedQuery = await baseQuery
                .GroupBy(s => s.ServiceKey)
                .Select(g => g.OrderByDescending(s => s.ServiceVersion).FirstOrDefault())
                .ToListAsync();

            var isTaskWithDeadline = openTask is "cert review" or "primary check" or "secondary check";                        

            Func<Service, bool> taskFilter = openTask switch
            {
                "cert review" => s =>
                    ((s.ServiceStatus == ServiceStatusEnum.Submitted || s.ServiceStatus == ServiceStatusEnum.Resubmitted) &&
                     s.ServiceStatus != ServiceStatusEnum.Removed &&
                     s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                     s.Id != s?.CertificateReview?.ServiceId) ||
                    (s.CertificateReview != null &&
                     (s.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.DeclinedByProvider )),
                "primary check" => s =>
                    ((s.ServiceStatus == ServiceStatusEnum.Received &&
                      s.ServiceStatus != ServiceStatusEnum.Removed &&
                      s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                      s.Id != s?.PublicInterestCheck?.ServiceId) ||
                     (s?.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer)),
                "secondary check" => s =>
                    (s.ServiceStatus != ServiceStatusEnum.Removed &&
                     s.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                     s.PublicInterestCheck != null &&
                     (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
                      s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed)),
                _ => s => s.ServiceStatus == ServiceStatusEnum.UpdatesRequested
                         || s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation
                         || s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation
            };

            var filtered = groupedQuery.Where(taskFilter).ToList();

            if (isTaskWithDeadline)
            {
                int deadline = openTask switch
                {
                    "cert review" => Constants.DaysLeftToCompleteCertificateReview,
                    _ => Constants.DaysLeftToCompletePICheck
                };

                var results = filtered.Select(s =>
                {
                    var baseDate = s.ModifiedTime ?? s.CreatedTime;
                    int daysLeft = 0;

                    if (baseDate.HasValue)
                    {
                        var daysPassed = (DateTime.UtcNow.Date - baseDate.Value.Date).Days;
                        daysLeft = Math.Max(0, deadline - daysPassed);
                    }

                    string appType = s.ServiceVersion == 1 ? Constants.NewApplication : Constants.ReApplication;

                    return new
                    {
                        Service = s,
                        DaysLeft = daysLeft,
                        NewOrResubmission = appType
                    };
                }).ToList();

                var sortedResults = sort switch
                {
                    "service name" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.Service.ServiceName)
                        : results.OrderBy(r => r.Service.ServiceName),

                    "provider" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.Service.Provider.RegisteredName)
                        : results.OrderBy(r => r.Service.Provider.RegisteredName),

                    "days" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.DaysLeft)
                        : results.OrderBy(r => r.DaysLeft),

                    "application" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.NewOrResubmission)
                        : results.OrderBy(r => r.NewOrResubmission)
                };

                var paged = sortedResults
                    .Skip((pageNumber - 1) * 10)
                    .Take(10)
                    .Select(r => r.Service)
                    .ToList();

                return new PaginatedResult<Service>
                {
                    Items = paged,
                    TotalCount = results.Count
                };
            }
            else
            {
                var results = filtered.Select(s => new
                {
                    Service = s,
                    Status = s.ServiceStatus,
                    NewOrResubmission = s.ServiceVersion == 1 ? Constants.NewApplication : Constants.ReApplication
                }).ToList();

                var priorityOrder = new List<ServiceStatusEnum>
                {
                    ServiceStatusEnum.CabAwaitingRemovalConfirmation,
                    ServiceStatusEnum.AwaitingRemovalConfirmation,
                    ServiceStatusEnum.UpdatesRequested
                };

                var sortedResults = sort switch
                {
                    "service name" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.Service.ServiceName)
                        : results.OrderBy(r => r.Service.ServiceName),

                    "provider" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.Service.Provider.RegisteredName)
                        : results.OrderBy(r => r.Service.Provider.RegisteredName),

                    "status" => sortAction == "descending"
                        ? results.OrderByDescending(r => priorityOrder.IndexOf(r.Status)) 
                        : results.OrderBy(r => priorityOrder.IndexOf(r.Status)),

                    "application" => sortAction == "descending"
                        ? results.OrderByDescending(r => r.NewOrResubmission)
                        : results.OrderBy(r => r.NewOrResubmission)
                };

                var paged = sortedResults
                    .Skip((pageNumber - 1) * 10)
                    .Take(10)
                    .Select(r => r.Service)
                    .ToList();

                return new PaginatedResult<Service>
                {
                    Items = paged,
                    TotalCount = results.Count
                };
            }
        }

        public async Task<Dictionary<string, int>> GetPendingCounts()
        {
            var latestServices = await _context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Include(s => s.PublicInterestCheck)
                .GroupBy(s => s.ServiceKey)
                .Select(g => g.OrderByDescending(s => s.ServiceVersion).First())
                .ToListAsync();

            int certificateReviewCount = 0;
            int primaryCount = 0;
            int secondaryCount = 0;
            int updateOrRemovalCount = 0;

            foreach (var s in latestServices)
            {
                bool notRemovedOrDraft = s.ServiceStatus != ServiceStatusEnum.Removed &&
                                         s.ServiceStatus != ServiceStatusEnum.SavedAsDraft;

                if (((s.ServiceStatus == ServiceStatusEnum.Submitted || s.ServiceStatus == ServiceStatusEnum.Resubmitted) &&
                      notRemovedOrDraft &&
                      s.Id != s?.CertificateReview?.ServiceId) ||
                    (s.CertificateReview != null &&
                     ( s.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)))
                {
                    certificateReviewCount++;
                }

                if ((s.ServiceStatus == ServiceStatusEnum.Received && notRemovedOrDraft &&
                      s.Id != s?.PublicInterestCheck?.ServiceId) ||
                     (s?.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer))
                {
                    primaryCount++;
                }

                if (notRemovedOrDraft &&
                    s.PublicInterestCheck != null &&
                    (s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
                     s.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed))
                {
                    secondaryCount++;
                }

                if (s.ServiceStatus == ServiceStatusEnum.UpdatesRequested ||
                    s.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation ||
                    s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation)
                {
                    updateOrRemovalCount++;
                }
            }

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
    }
}
