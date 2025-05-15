using AutoMapper;
using DVSAdmin.CommonUtility.JWT;
using Microsoft.Extensions.Configuration;
using DVSAdmin.Data.Repositories;
using DVSAdmin.BusinessLogic.Models;

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

    }
}
