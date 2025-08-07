using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class HomeService: IHomeService
    {
        private readonly IHomeRepository homeRepository;
        private readonly IMapper automapper;

        public HomeService(IHomeRepository homeRepository, IUserService userService, IMapper automapper, IJwtService jwtService, IConfiguration configuration)
        {
            this.homeRepository = homeRepository;
            this.automapper = automapper;
        }

        //get certificate reviews list
        //get primary checks list

        public async Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber, string sort, string sortAction, string openTask)
        {
            var paginatedServices = await homeRepository.GetServices(pageNumber, sort, sortAction, openTask);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };
        }
        public Task<UserDto> GetUserByEmail(string userEmail)
        {
            return homeRepository.GetUserByEmail(userEmail)
                .ContinueWith(task => automapper.Map<UserDto>(task.Result));
        }
    }
}
