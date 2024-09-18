using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPublicInterestCheckService
    {
      
        public Task<List<ServiceDto>> GetPICheckList();
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType);
        public Task<ServiceDto> GetServiceDetailsWithMappings(int serviceId);

    }
}