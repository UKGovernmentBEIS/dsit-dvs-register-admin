using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories
{
    public class CabTransferRepository: ICabTransferRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<CabTransferRepository> logger;

        public CabTransferRepository(DVSAdminDbContext context, ILogger<CabTransferRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public class PaginatedResult<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
        }

        public async Task<PaginatedResult<Service>> GetServices(int pageNumber)
        {
            // needs fixing
            var query = context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Include(s => s.ServiceRoleMapping)
                .Include(s => s.CabUser).ThenInclude(s => s.Cab)
                .Where(s => s.ServiceStatus == ServiceStatusEnum.Published || s.ServiceStatus == ServiceStatusEnum.Removed)
                .GroupBy(s => s.ServiceKey)
                .Select(g => g.OrderByDescending(s => s.ServiceVersion).FirstOrDefault())
                .OrderBy(s => s.ServiceStatus)
                .ThenByDescending(s => s.ModifiedTime);

            var totalCount = await query.CountAsync(); 

            var items = await query
                .Skip((pageNumber - 1) * 10)
                .Take(10)
                .ToListAsync();

            return new PaginatedResult<Service>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
