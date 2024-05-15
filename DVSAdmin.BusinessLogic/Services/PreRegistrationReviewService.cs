using AutoMapper;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;

namespace DVSAdmin.BusinessLogic.Services
{
    public class PreRegistrationReviewService : IPreRegistrationReviewService
    {
        private readonly IPreRegistrationReviewRepository preRegistrationReviewRepository;
        private readonly IMapper automapper;
        private readonly ILogger<PreRegistrationReviewService> logger;
        private readonly IEmailSender emailSender;

        public PreRegistrationReviewService(IPreRegistrationReviewRepository preRegistrationReviewRepository, IMapper automapper,
          ILogger<PreRegistrationReviewService> logger, IEmailSender emailSender)
        {
            this.preRegistrationReviewRepository = preRegistrationReviewRepository;
            this.automapper = automapper;
            this.logger = logger;
            this.emailSender = emailSender;
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


            PreRegistration preRegistration = await preRegistrationReviewRepository.GetPreRegistration(preRegistrationReviewDto.PreRegistrationId);

            PreRegistrationReview preRegistrationReview = new PreRegistrationReview();
            automapper.Map(preRegistrationReviewDto, preRegistrationReview);
            GenericResponse genericResponse = await preRegistrationReviewRepository.SavePreRegistrationReview(preRegistrationReview, reviewType);

            if (genericResponse.Success)
            {
                DateTime expirationdate = Convert.ToDateTime(preRegistration.CreatedDate).AddDays(Constants.DaysLeftToComplete);
                string expirationDate = Helper.GetLocalDateTime(expirationdate, "d MMM yyyy h:mm tt");
                if (reviewType == ReviewTypeEnum.PrimaryCheck)
                {
                    if (preRegistrationReviewDto.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckPassed)
                    {
                        await emailSender.SendPrimaryCheckPassConfirmationToOfDia(preRegistration.URN??string.Empty, expirationDate);
                    }
                    else if (preRegistrationReviewDto.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckFailed)
                    {
                        await emailSender.SendPrimaryCheckFailConfirmationToOfDia(preRegistration.URN??string.Empty, expirationDate);
                    }

                }
                else if (reviewType == ReviewTypeEnum.SecondaryCheck)
                {

                    if (preRegistrationReviewDto.ApplicationReviewStatus == ApplicationReviewStatusEnum.SentBackBySecondReviewer)
                    {
                        await emailSender.SendPrimaryCheckRoundTwoConfirmationToOfDia(preRegistration.URN??string.Empty, expirationDate);
                    }
                    else if (preRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationRejected)
                    {
                        UniqueReferenceNumber uniqueReferenceNumber = new UniqueReferenceNumber
                        {
                            URNStatus = URNStatusEnum.Rejected,
                            PreRegistrationId = preRegistrationReviewDto.PreRegistrationId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        genericResponse = await preRegistrationReviewRepository.UpdateURNStatus(uniqueReferenceNumber);
                        await emailSender.SendPrimaryApplicationRejectedConfirmationToOfDia(preRegistration.URN??string.Empty);
                        await emailSender.SendApplicationRejectedToDIASP(preRegistration.FullName, preRegistration.Email);
                        if (!string.IsNullOrEmpty(preRegistration.SponsorEmail))
                        {
                            await emailSender.SendApplicationRejectedToDIASP(preRegistration.SponsorFullName??string.Empty, preRegistration.SponsorEmail);
                        }
                    }
                    else if (preRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationApproved)
                    {

                        UniqueReferenceNumber uniqueReferenceNumber = new UniqueReferenceNumber
                        {
                            URNStatus = URNStatusEnum.Approved,
                            PreRegistrationId = preRegistrationReviewDto.PreRegistrationId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        genericResponse = await preRegistrationReviewRepository.UpdateURNStatus(uniqueReferenceNumber);


                        await emailSender.SendURNIssuedConfirmationToOfDia(preRegistration.URN??string.Empty); //email to ofdia                       
                        await emailSender.SendApplicationApprovedToDIASP(preRegistration.FullName, preRegistration.URN??string.Empty, //email to provider
                        GetUrnExpiryDate(uniqueReferenceNumber.ModifiedDate), preRegistration.Email);
                        if (!string.IsNullOrEmpty(preRegistration.SponsorEmail))
                        {
                            await emailSender.SendApplicationApprovedToDIASP(preRegistration.SponsorFullName??string.Empty, preRegistration.URN??string.Empty,
                            GetUrnExpiryDate(uniqueReferenceNumber.ModifiedDate), preRegistration.SponsorEmail);
                        }
                    }
                }
            }
            return genericResponse;
        }

        private string GetUrnExpiryDate(DateTime? dateTime)
        {
            DateTime expirationdate = Convert.ToDateTime(dateTime).AddDays(Constants.URNExpiryDays);
            string expirationDate = Helper.GetLocalDateTime(expirationdate, "dd MMM yyyy h:mm tt");
            return expirationDate;
        }
    }
}
