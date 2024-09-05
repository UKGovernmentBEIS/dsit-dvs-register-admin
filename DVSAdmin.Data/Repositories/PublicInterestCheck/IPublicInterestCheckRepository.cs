using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories.PublicInterestCheck
{
    public interface IPublicInterestCheckRepository
    {
        public Task<List<Service>> GetPICheckList();
    }
}
