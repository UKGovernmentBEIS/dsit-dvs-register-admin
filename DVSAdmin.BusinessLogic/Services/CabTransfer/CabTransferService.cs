using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services.CabTransfer
{
    public class CabTransferService: ICabTransferService
    {
        private readonly ICabTransferRepository cabTransferRepository;
        private readonly IMapper automapper;
        private readonly CabTransferEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;

        public CabTransferService(ICabTransferRepository cabTransferRepository, IMapper automapper, CabTransferEmailSender emailSender, IJwtService jwtService, IConfiguration configuration)
        {
            this.cabTransferRepository = cabTransferRepository;
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

        public Task<GenericResponse> ReassignServiceAsync(int serviceId, int newCabId, string userEmail)
        {
            // TODO: database update here…
            return Task.FromResult(new GenericResponse { Success = true });
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
        public async Task<GenericResponse> SaveCabTransferRequest(CabTransferRequestDto cabTransferRequestDto, string loggedInUserEmail)
        {
            CabTransferRequest cabTransferRequest = new();
            automapper.Map(cabTransferRequestDto, cabTransferRequest);
            GenericResponse genericResponse = await cabTransferRepository.SaveCabTransferRequest(cabTransferRequest, loggedInUserEmail);
            if (genericResponse.Success)
            {
                await emailSender.SendCabTransferConfirmationToDSTI(cabTransferRequestDto.ToCab.Cab.CabName, cabTransferRequestDto.ProviderProfile.RegisteredName, 
                    cabTransferRequestDto.Service.ServiceName);
                await emailSender.SendCabTransferConfirmationToCAB(cabTransferRequestDto.ToCab.Cab.CabName, cabTransferRequestDto.ProviderProfile.RegisteredName,
                    cabTransferRequestDto.Service.ServiceName, cabTransferRequestDto.ToCab.CabEmail);

            }
            return genericResponse;
        }

        public async Task<GenericResponse> CancelCabTransferRequest(int cabTransferRequestId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await cabTransferRepository.CancelCabTransferRequest(cabTransferRequestId, loggedInUserEmail);
            return genericResponse;
        }

    }
}