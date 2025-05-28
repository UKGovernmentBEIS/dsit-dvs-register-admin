using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services.CabTransfer
{
    public class CabTransferService: ICabTransferService
    {
        private readonly ICabTransferRepository cabTransferRepository;
        private readonly IRemoveProviderService removeProviderService;
        private readonly IMapper automapper;
        private readonly CabTransferEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;
        private readonly IUserService userService;


        public CabTransferService(ICabTransferRepository cabTransferRepository, IRemoveProviderService removeProviderService, IUserService userService, IMapper automapper, CabTransferEmailSender emailSender, IJwtService jwtService, IConfiguration configuration)
        {
            this.cabTransferRepository = cabTransferRepository;
            this .removeProviderService = removeProviderService;
            this.userService           = userService;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.jwtService = jwtService;
            this.configuration = configuration;
        }

        public async Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber, string searchText = "")
        {
            var paginatedServices = await cabTransferRepository.GetServices(pageNumber, searchText);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }
        
        public async Task<IReadOnlyList<CabDto>> ListCabsExceptCurrentAsync(int serviceId)
        {
            var service = await cabTransferRepository.GetServiceDetails(serviceId);
            var allCabs = await cabTransferRepository.GetAllCabsAsync();
            return allCabs
                .Where(c => c.Id != service.CabUser.CabId)
                .Select(c => new CabDto { Id = c.Id, CabName = c.CabName })
                .ToList()
                .AsReadOnly();
        }
        
        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            Service service = await cabTransferRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(service);
            return serviceDto;
        }
        public async Task<CabTransferRequestDto> GetCabTransferDetails(int serviceId)
        {
            CabTransferRequest cabTransferRequest = await cabTransferRepository.GetCabTransferDetails(serviceId);
            CabTransferRequestDto cabTransferRequestDto = automapper.Map<CabTransferRequestDto>(cabTransferRequest);
            return cabTransferRequestDto;
        }
        public async Task<GenericResponse> SaveCabTransferRequest(CabTransferRequestDto cabTransferRequestDto,string serviceName, string providerName, string loggedInUserEmail)
        {
            CabTransferRequest cabTransferRequest = new();
            automapper.Map(cabTransferRequestDto, cabTransferRequest);
            GenericResponse genericResponse = await cabTransferRepository.SaveCabTransferRequest(cabTransferRequest, loggedInUserEmail);
            
            if (genericResponse.Success)
            {
                await removeProviderService.UpdateProviderStatusByStatusPriority(cabTransferRequestDto.ProviderProfileId, loggedInUserEmail, EventTypeEnum.InitiateCabTranferRequest);

                List<CabUser> activeCabUsers = await cabTransferRepository.GetActiveCabUsers(cabTransferRequestDto.ToCabId);
                var cabName = activeCabUsers.FirstOrDefault()?.Cab.CabName??string.Empty;
                
                await emailSender.SendCabTransferConfirmationToDSTI(cabName, providerName, serviceName);              
                foreach(var user in activeCabUsers)
                {                 
                    await emailSender.SendCabTransferConfirmationToCAB(cabName, providerName, serviceName, user.CabEmail);
                }
            }
            return genericResponse;
        }

        public async Task<GenericResponse> CancelCabTransferRequest(int cabTransferRequestId, string serviceName, string providerName,int toCabId, int providerId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await cabTransferRepository.CancelCabTransferRequest(cabTransferRequestId, loggedInUserEmail);
            if (genericResponse.Success)
            {
                await removeProviderService.UpdateProviderStatusByStatusPriority(providerId, loggedInUserEmail, EventTypeEnum.CancelCabTransferRequest);

                List<CabUser> activeCabUsers = await cabTransferRepository.GetActiveCabUsers(toCabId);
                var cabName = activeCabUsers.FirstOrDefault()?.Cab.CabName ?? string.Empty;

                await emailSender.SendCabTransferCancellationToDSTI(providerName, serviceName);
                foreach (var user in activeCabUsers)
                {
                    await emailSender.SendCabTransferCancellationToCAB(cabName, providerName, serviceName, user.CabEmail);
                }
            }
            return genericResponse;
        }
    }
}