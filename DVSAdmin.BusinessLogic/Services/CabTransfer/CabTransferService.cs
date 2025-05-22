using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
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
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;

        public CabTransferService(ICabTransferRepository cabTransferRepository, IMapper automapper, IJwtService jwtService, IConfiguration configuration)
        {
            this.cabTransferRepository = cabTransferRepository;
            this.automapper = automapper;
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
            return genericResponse;
        }

        public async Task<GenericResponse> CancelCabTransferRequest(int cabTransferRequestId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await cabTransferRepository.CancelCabTransferRequest(cabTransferRequestId, loggedInUserEmail);
            return genericResponse;
        }

    }
}
