using AutoMapper;
using DVSAdmin.Data.Repositories;
using DVSAdmin.BusinessLogic.Models.PreRegistration;
using Microsoft.Extensions.Logging;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

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
        public async Task<PreRegistrationDto> GetPreRegistration(int preRegistrationId)
        {
            var preRegistration = await preRegistrationReviewRepository.GetPreRegistration(preRegistrationId);
            return automapper.Map<PreRegistrationDto>(preRegistration);
        }

        public async Task<List<PreRegistrationDto>> GetPreRegistrations()
        {
            var preRegistrations = await preRegistrationReviewRepository.GetPreRegistrations();
            return automapper.Map<List<PreRegistrationDto>>(preRegistrations);
        }

        public async Task<GenericResponse> SavePreRegistrationReview(PreRegistrationReviewDto preRegistrationReviewDto, ReviewTypeEnum reviewType)
        {
            PreRegistrationReview preRegistrationReview = new PreRegistrationReview();
            automapper.Map(preRegistrationReviewDto, preRegistrationReview);
            GenericResponse genericResponse = await preRegistrationReviewRepository.SavePreRegistrationReview(preRegistrationReview, reviewType);
            return genericResponse;
        }
    }
}
