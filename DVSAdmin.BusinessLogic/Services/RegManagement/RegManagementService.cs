using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RegManagementService : IRegManagementService
    {
        private readonly IRegManagementRepository regManagementRepository;        
        private readonly IMapper automapper;       
   
      

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;          
           
        }
        public async Task<List<ProviderProfileDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();

            List<ProviderProfileDto> providersList = automapper.Map<List<ProviderProfileDto>>(providers);

            if(providersList!=null && providersList.Count()>0)
            {
                providersList = providersList.Select(providerDto =>
                {
                    providerDto.Services = ServiceHelper.FilterByServiceStatusAndLatestModifiedDate(providerDto.Services);                    
                    return providerDto;
                }).ToList();
            }
          

            return providersList;
        }

        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var service = await regManagementRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(service);
            return serviceDto;
        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerProfileId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerProfileId);            
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            providerDto.Services = ServiceHelper.FilterByServiceStatusAndLatestModifiedDate(providerDto.Services);
            return providerDto;
        }

     

        public async Task<List<string>> GetCabEmailListForServices(List<int> serviceIds)
        {
            return await regManagementRepository.GetCabEmailListForServices(serviceIds);
        }



        public async Task<List<ServiceDto>> GetServiceVersionList(int serviceKey)
        {
            var serviceList = await regManagementRepository.GetServiceVersionList(serviceKey);
            List<ServiceDto> services =  automapper.Map<List<ServiceDto>>(serviceList);
            return ServiceHelper.FilterByServiceStatus(services);

        }
        public async Task<PaginatedResult<ServiceDto>>  GetServiceHistory(int pageNumber, string sort, string sortAction)
        {
            var paginatedServices = await regManagementRepository.GetServiceHistory(pageNumber,sort,sortAction);
            var serviceDtos = automapper.Map<List<ServiceDto>>(paginatedServices.Items);

            return new PaginatedResult<ServiceDto>
            {
                Items = serviceDtos,
                TotalCount = paginatedServices.TotalCount
            };

        }

        


    }
}
