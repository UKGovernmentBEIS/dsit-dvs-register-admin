using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobRepository backgroundJobRepository;
        private readonly IRemoveProviderService removeProviderService;

        
        public BackgroundJobService(IBackgroundJobRepository backgroundJobRepository, IRemoveProviderService removeProviderService)
        {
            this.backgroundJobRepository = backgroundJobRepository ?? throw new ArgumentNullException(nameof(backgroundJobRepository));
            this.removeProviderService = removeProviderService ?? throw new ArgumentNullException();
        }

        public async Task RemoveExpiredCertificates()
        {
            var expiredServiceList = await backgroundJobRepository.GetExpiredCertificates();

            if (expiredServiceList != null)
            {
                var expiredServiceIds = expiredServiceList.Select(service => service.Id).ToList();

                if (expiredServiceIds.Any())
                {
                    bool success = await backgroundJobRepository.MarkAsRemoved(expiredServiceIds);

                    foreach (var service in expiredServiceList)
                    {
                        Console.WriteLine($"Service: {service.ServiceName} has an expired certificate and has been removed from the Register");
                        if (success)
                        {                          
                            GenericResponse genericResponse = await removeProviderService.UpdateProviderStatusByStatusPriority(service.ProviderProfileId, TeamEnum.CronJob.ToString(), EventTypeEnum.RemovedByCronJob, TeamEnum.CronJob);
                            if (genericResponse.Success)
                            {
                                Console.WriteLine($"Provider: {service.Provider.RegisteredName} status updated by priority");
                            }

                        }
                    }
                }

            }
        }
    }
}