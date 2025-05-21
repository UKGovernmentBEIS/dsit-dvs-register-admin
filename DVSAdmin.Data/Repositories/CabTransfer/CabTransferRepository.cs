using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

        public async Task<PaginatedResult<Service>> GetServices(int pageNumber, string searchText = "")
        {
            var baseQuery = context.Service
                .Include(s => s.Provider)
                .Include(s => s.CertificateReview)
                .Include(s => s.ServiceRoleMapping)
                .Include(s => s.CabUser).ThenInclude(s => s.Cab)
                .Where(s => s.ServiceStatus == ServiceStatusEnum.Published || s.ServiceStatus == ServiceStatusEnum.Removed);


            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim().ToLower();
                baseQuery = baseQuery.Where(s => EF.Functions.Like(s.ServiceName.ToLower(), "%"+searchText.ToLower()+"%"));
            }

            var groupedQuery = await baseQuery
                .GroupBy(s => s.ServiceKey)
                .Select(g => g.OrderByDescending(s => s.ServiceVersion).FirstOrDefault())
                .ToListAsync();

            var orderedQuery = groupedQuery
                .OrderBy(s => s.ServiceStatus)
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



        public async Task<GenericResponse> SaveCabTransferRequest(CabTransferRequest cabTransferRequest, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingRequest = await context.CabTransferRequest.Include(s => s.Service).Where(s => s.ServiceId == cabTransferRequest.ServiceId &&
                s.ProviderProfileId == cabTransferRequest.ProviderProfileId &&
                s.ToCabId == cabTransferRequest.ToCabId &&
                s.FromCabUserId == cabTransferRequest.FromCabUserId
                && s.RequestManagement != null && s.RequestManagement.RequestStatus == RequestStatusEnum.Pending).ToListAsync();

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
                        service.ServiceStatus = ServiceStatusEnum.PublishedUnderRassign;
                      
                    }
                    else if(currentStatus == ServiceStatusEnum.Removed)
                    {
                        service.ServiceStatus = ServiceStatusEnum.RemovedUnderRassign;
                        
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
