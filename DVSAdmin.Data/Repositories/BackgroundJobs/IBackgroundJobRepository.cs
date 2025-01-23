using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IBackgroundJobRepository
    {
        
        public Task<List<Service>> GetExpiredCertificates();
        Task MarkAsRemoved(List<int>? serviceIds);
    }
}