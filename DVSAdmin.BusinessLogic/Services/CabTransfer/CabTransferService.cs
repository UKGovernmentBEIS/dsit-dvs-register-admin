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

        public class PaginatedResult<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
        }

        public async Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber)
        {
            var paginatedServices = await cabTransferRepository.GetServices(pageNumber);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }

  
        public async Task<GenericResponse> SaveCabTransferRequest(CabTransferRequestDto cabTransferRequestDto, string loggedInUserEmail)
        {
            CabTransferRequest cabTransferRequest = new();
            automapper.Map(cabTransferRequestDto, cabTransferRequest);
            GenericResponse genericResponse = await cabTransferRepository.SaveCabTransferRequest(cabTransferRequest, loggedInUserEmail);
            return genericResponse;
        }

        public async Task<GenericResponse> CancelCabTransferRequest(int cantransferRequestId, string loggedInUserEmail)
        {

            GenericResponse genericResponse = await cabTransferRepository.CancelCabTransferRequest(cantransferRequestId, loggedInUserEmail);
            return genericResponse;
        }

    }
}
