using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobRepository backgroundJobRepository;
        
        public BackgroundJobService(IBackgroundJobRepository backgroundJobRepository)
        {
            this.backgroundJobRepository = backgroundJobRepository ?? throw new ArgumentNullException(nameof(backgroundJobRepository));
        }
               
        public async Task RemoveExpiredCertificates()
        {
            var expiredServiceList = await backgroundJobRepository.GetExpiredCertificates();
            var expiredServiceIds = expiredServiceList.Select(service => service.Id).ToList();

            if (expiredServiceIds.Any())
            {
                await backgroundJobRepository.MarkAsRemoved(expiredServiceIds);
                foreach (var service in expiredServiceList)
                {
                    Console.WriteLine(service.ServiceName);
                }
            }
        }
    }
}