using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobRepository backgroundJobRepository;
        private readonly IRemoveProviderRepository removeProviderRepository;

        
        public BackgroundJobService(IBackgroundJobRepository backgroundJobRepository, IRemoveProviderRepository removeProviderRepository)
        {
            this.backgroundJobRepository = backgroundJobRepository ?? throw new ArgumentNullException(nameof(backgroundJobRepository));
            this.removeProviderRepository = removeProviderRepository ?? throw new ArgumentNullException();
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
                            ProviderProfile providerProfile = await removeProviderRepository.GetProviderAndServices(service.ProviderProfileId);
                            // update provider status
                            ProviderStatusEnum providerStatus = ServiceHelper.GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
                            GenericResponse genericResponse = await removeProviderRepository.UpdateProviderStatus(providerProfile.Id, providerStatus, TeamEnum.CronJob.ToString(), EventTypeEnum.RemovedByCronJob, TeamEnum.CronJob);
                            if (genericResponse.Success)
                            {
                                Console.WriteLine($"Provider: {providerProfile.RegisteredName} has no valid services and been removed from the Register");
                            }

                        }
                    }
                }

            }
        }
    }
}