using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IPublicInterestCheckService
    {
      
        public Task<List<ServiceDto>> GetPICheckList();
        public Task<ServiceDto> GetServiceDetails(int serviceId);       
        public Task<ServiceDto> GetServiceDetailsWithMappings(int serviceId);   
     
        public Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType, string loggedInUserEmail);
        public Task<ServiceDto> GetServiceDetailsForPublishing(int serviceId);       
    }
}