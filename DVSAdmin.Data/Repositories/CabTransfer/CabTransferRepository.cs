using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
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

        public async Task<List<CabUser>> GetActiveCabUsers(int cabId)
        {
           return await context.CabUser.Include(s=>s.Cab).Where(s => s.CabId ==cabId && s.IsActive).ToListAsync();
           
        }

        public async Task<PaginatedResult<Service>> GetServices(int pageNumber, string searchText = "")
        {
            var baseQuery = context.Service
                .Include(s => s.Provider)
                .Include(s => s.CabUser).ThenInclude(s => s.Cab)
                .Include(s => s.CabTransferRequest).ThenInclude(s => s.RequestManagement);

            var groupedQuery = await baseQuery
                .GroupBy(s => s.ServiceKey)
                .Select(g => g.OrderByDescending(s => s.ServiceVersion).FirstOrDefault())
                .ToListAsync();

            var filteredQuery = groupedQuery
            .Where(s =>
                (s.ServiceStatus == ServiceStatusEnum.Published ||
                 s.ServiceStatus == ServiceStatusEnum.Removed) &&
                !(s.CabTransferRequest?.Any(c => c.CertificateUploaded == false && c.RequestManagement?.RequestStatus == RequestStatusEnum.Approved) ?? false) ||
                s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                s.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign);


            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim().ToLower();
                filteredQuery = filteredQuery
                    .Where(s => s.ServiceName.ToLower().Contains(searchText))
                    .ToList();
            }

            var priorityOrder = new List<ServiceStatusEnum>
            {
                ServiceStatusEnum.PublishedUnderReassign,
                ServiceStatusEnum.Published,
                ServiceStatusEnum.RemovedUnderReassign,
                ServiceStatusEnum.Removed
            };

            var orderedQuery = filteredQuery
                .OrderBy(s => priorityOrder.IndexOf(s.ServiceStatus))
                .ThenBy(s => s.ModifiedTime);

            var totalCount = orderedQuery.Count();

            var items = orderedQuery
                .Skip((pageNumber - 1) * 10)
                .Take(10)
                .ToList();

            return new PaginatedResult<Service>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        //ToDo : move to cabtranfser repository

        public Task<List<Cab>> GetAllCabsAsync()
            => context.Cabs
                .OrderBy(c => c.CabName)
                .ToListAsync();

        public Task<Cab> GetCabByIdAsync(int cabId)
            => context.Cabs.FindAsync(cabId).AsTask();
        
        public async Task<Service> GetServiceDetails(int serviceId)
        {
            var service = await context.Service
                .Include(s => s.Provider)
                .Include(s => s.CabUser)
                    .ThenInclude(c => c.Cab)
                .Include(s => s.ServiceQualityLevelMapping)
                    .ThenInclude(s => s.QualityLevel)
                .Include(s => s.ServiceIdentityProfileMapping)
                    .ThenInclude(s => s.IdentityProfile)
                .Include(s => s.ServiceRoleMapping)
                    .ThenInclude(s => s.Role)
                .Include(s => s.ServiceSupSchemeMapping)
                    .ThenInclude(s => s.SupplementaryScheme)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            return service;
        }

        public async Task<CabTransferRequest> GetCabTransferDetails(int serviceId)
        {
            var cabTransferRequest = await context.CabTransferRequest
                .Include(c => c.Service).ThenInclude(c=>c.Provider)
                .Include(c => c.Service)
                    .ThenInclude(s => s.ServiceIdentityProfileMapping)
                        .ThenInclude(ip => ip.IdentityProfile)
                .Include(c => c.Service)
                    .ThenInclude(s => s.ServiceRoleMapping)
                        .ThenInclude(r => r.Role)
                .Include(c => c.Service)
                    .ThenInclude(s => s.ServiceSupSchemeMapping)
                        .ThenInclude(ss => ss.SupplementaryScheme)
                .Include(c => c.Service)
                    .ThenInclude(s => s.ServiceQualityLevelMapping)
                        .ThenInclude(ql => ql.QualityLevel)
                .Include(c => c.FromCabUser)
                    .ThenInclude(c => c.Cab)
                .Include(c => c.ToCab)
                .FirstOrDefaultAsync(c => c.ServiceId == serviceId);

            return cabTransferRequest;
        }



        public async Task<GenericResponse> SaveCabTransferRequest(CabTransferRequest cabTransferRequest, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingRequest = await context.CabTransferRequest.Include(s => s.Service).Where(s => s.ServiceId == cabTransferRequest.ServiceId &&         
                s.ToCabId == cabTransferRequest.ToCabId &&
                s.FromCabUserId == cabTransferRequest.FromCabUserId
                && s.RequestManagement != null && (s.RequestManagement.RequestStatus == RequestStatusEnum.Pending)).ToListAsync();

                if (existingRequest != null && existingRequest.Count > 0)
                {
                    genericResponse.Success = false;
                    await transaction.RollbackAsync();
                }
                else
                {
                    var service = await context.Service.FirstOrDefaultAsync(s => s.Id == cabTransferRequest.ServiceId);
                    if (service == null || ( service.ServiceStatus != ServiceStatusEnum.Published && service.ServiceStatus != ServiceStatusEnum.Removed))
                        throw new InvalidDataException("Invalid service details");
                    
                    ServiceStatusEnum currentStatus = service.ServiceStatus;    
                    if (currentStatus == ServiceStatusEnum.Published) 
                    {
                        service.ServiceStatus = ServiceStatusEnum.PublishedUnderReassign;
                      
                    }
                    else if(currentStatus == ServiceStatusEnum.Removed)
                    {
                        service.ServiceStatus = ServiceStatusEnum.RemovedUnderReassign;
                        
                    }
                    service.ModifiedTime = DateTime.UtcNow;
                   

                    cabTransferRequest.DecisionTime = DateTime.UtcNow;
                    cabTransferRequest.RequestManagement.ModifiedTime = DateTime.UtcNow;

                    await context.CabTransferRequest.AddAsync(cabTransferRequest);
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.InitiateCabTranferRequest, loggedInUserEmail);
                    await transaction.CommitAsync();
                    genericResponse.Success = true;
                }


            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                await transaction.RollbackAsync();
                logger.LogError("SaveCabTransferRequest failed with {exception} ", ex.Message);

            }
            return genericResponse;
        }


        public async Task<GenericResponse> CancelCabTransferRequest(int cabtransferRequestId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {

                var cabTransferRequest = await context.CabTransferRequest.Include(c => c.RequestManagement).Include(s => s.Service).FirstOrDefaultAsync(s => s.Id == cabtransferRequestId && s.RequestManagement != null
                && s.RequestManagement.RequestStatus == RequestStatusEnum.Pending);

                if (cabTransferRequest != null && cabTransferRequest.RequestManagement != null)
                {
                    cabTransferRequest.Service.ServiceStatus = cabTransferRequest.PreviousServiceStatus;
                    context.RequestManagement.Remove(cabTransferRequest.RequestManagement);
                    context.CabTransferRequest.Remove(cabTransferRequest);
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CancelCabTransferRequest, loggedInUserEmail);
                    await transaction.CommitAsync();
                    genericResponse.Success = true;

                }
                else
                {
                    await transaction.RollbackAsync();
                    genericResponse.Success = false;
                }
            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                await transaction.RollbackAsync();
                logger.LogError("SaveCabTransferRequest failed with {exception} ", ex.Message);
            }
            return genericResponse;
        }
    }
}