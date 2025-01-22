using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IBackgroundJobService
    {
        public Task RemoveExpiredCertificates();
    }
}