using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPublicInterestCheckService
    {
      
        public Task<List<Service>> GetPICheckList();
        
    }
}