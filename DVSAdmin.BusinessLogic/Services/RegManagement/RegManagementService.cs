using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RegManagementService : IRegManagementService
    {
        private readonly IRegManagementRepository regManagementRepository;
        private readonly IMapper automapper;
        private readonly ILogger<PreRegistrationReviewService> logger;
        private readonly IEmailSender emailSender;

        public RegManagementService(IRegManagementRepository regManagementRepository, IMapper automapper,
          ILogger<PreRegistrationReviewService> logger, IEmailSender emailSender)
        {
            this.regManagementRepository = regManagementRepository;
            this.automapper = automapper;
            this.logger = logger;
            this.emailSender = emailSender;
        }        
         public async Task<List<ProviderDto>> GetProviders()
        {
            var providers = await regManagementRepository.GetProviders();
            return automapper.Map<List<ProviderDto>>(providers);
        }

        public async Task<ProviderDto> GetProviderDetails(int providerId)
        {
            var provider = await regManagementRepository.GetProviderDetails(providerId);
            ProviderDto providerDto = automapper.Map<ProviderDto>(provider);
            return providerDto;
        }

    }
}
