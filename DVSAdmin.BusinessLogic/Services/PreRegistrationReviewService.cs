using AutoMapper;
using DVSAdmin.Data.Repositories;
using DVSAdmin.BusinessLogic.Models.PreRegistration;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.BusinessLogic.Services
{
    public class PreRegistrationReviewService : IPreRegistrationReviewService
    {
        private readonly IPreRegistrationReviewRepository preRegistrationReviewRepository;
        private readonly IMapper automapper;      
        private readonly ILogger<PreRegistrationReviewService> logger;

        public PreRegistrationReviewService(IPreRegistrationReviewRepository preRegistrationReviewRepository, IMapper automapper,
          ILogger<PreRegistrationReviewService> logger)
        {
            this.preRegistrationReviewRepository = preRegistrationReviewRepository;
            this.automapper = automapper;
            this.logger = logger;
        }
        public  async Task<List<PreRegistrationDto>> GetPreRegistrations()
        {
            var preRegistrations = await preRegistrationReviewRepository.GetPreRegistrations();
            return automapper.Map<List<PreRegistrationDto>>(preRegistrations);
        }
    }
}
