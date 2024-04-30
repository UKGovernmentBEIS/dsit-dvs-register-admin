using AutoMapper;
using DVSAdmin.Data.Repositories;
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
            var countries = await preRegistrationReviewRepository.GetCountries();
            PreRegistrationDto preRegistrationDto = automapper.Map<PreRegistrationDto>(preRegistration);
            List<CountryDto> countryDtos = automapper.Map<List<CountryDto>>(countries);
            var filteredMapping = preRegistrationDto.PreRegistrationCountryMappings.Where(x => x.PreRegistrationId == preRegistrationId);
            var countryIds = filteredMapping.Select(mapping => mapping.CountryId);
            preRegistrationDto.Countries = countryDtos.Where(country => countryIds.Contains(country.Id)).ToList();

            preRegistrationDto.CountrySubList = preRegistrationDto.Countries.Select((country, index) => new { Country = country, Index = index })
                          .GroupBy(x => x.Index / 15)
                          .Select(group => group.Select(x => x.Country).ToList())
                          .ToList();

            return preRegistrationDto;
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
            if (genericResponse.Success && reviewType == ReviewTypeEnum.SecondaryCheck &&
            (preRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationRejected ||
             preRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationApproved)) // update URN status for Approved and rejected applications
            {
                URNStatusEnum uRNStatus = preRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationApproved ? URNStatusEnum.Approved : URNStatusEnum.Rejected;
                UniqueReferenceNumber uniqueReferenceNumber = new UniqueReferenceNumber { URNStatus = uRNStatus, PreRegistrationId = preRegistrationReviewDto.PreRegistrationId };
                genericResponse = await preRegistrationReviewRepository.UpdateURNStatus(uniqueReferenceNumber);
            }
            return genericResponse;
        }
    }
}
