using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPublicInterestCheckService
    {
      
        public Task<List<PublicInterestCheckDto>> GetPICheckList();
        
    }
}