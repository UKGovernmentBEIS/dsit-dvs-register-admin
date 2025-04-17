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
        public Task<ServiceDto> GetProviderAndCertificateDetailsByConsentToken(string token, string tokenId);
        public Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheckDto publicInterestCheckDto, ReviewTypeEnum reviewType, string loggedInUserEmail);
        public Task<GenericResponse> GenerateTokenAndSendEmail(ServiceDto service, string loggedInUserEmail, bool isResend);

    }
}