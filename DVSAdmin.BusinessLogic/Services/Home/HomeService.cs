using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class HomeService: IHomeService
    {
        private readonly IHomeRepository homeRepository;
        private readonly IMapper automapper;

        public HomeService(IHomeRepository homeRepository, IMapper automapper)
        {
            this.homeRepository = homeRepository;
            this.automapper = automapper;
        }

        //get certificate reviews list
        //get primary checks list

        public async Task<PaginatedResult<ServiceDto>> GetPendingCertificateReviews( int pageNumber, string sort, string sortAction)
        {
            var paginatedServices = await homeRepository.GetPendingCertificateReviews(pageNumber, sort, sortAction);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }

        public async Task<PaginatedResult<ServiceDto>> GetPendingPrimaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction)
        {
            var paginatedServices = await homeRepository.GetPendingPrimaryChecks(loggedInUserEmail,pageNumber, sort, sortAction);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }
        public async Task<PaginatedResult<ServiceDto>> GetPendingSecondaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction)
        {
            var paginatedServices = await homeRepository.GetPendingSecondaryChecks(loggedInUserEmail, pageNumber, sort, sortAction);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }
        public async Task<PaginatedResult<ServiceDto>> GetPendingRequests(string loggedInUserEmail, int pageNumber, string sort, string sortAction)
        {
            var paginatedServices = await homeRepository.GetPendingRequests(loggedInUserEmail, pageNumber, sort, sortAction);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }       

        public async Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail)
        {
            return await homeRepository.GetPendingCounts(loggedInUserEmail);
        }
        public Task<UserDto> GetUserByEmail(string userEmail)
        {
            return homeRepository.GetUserByEmail(userEmail)
                .ContinueWith(task => automapper.Map<UserDto>(task.Result));
        }
    }
}
