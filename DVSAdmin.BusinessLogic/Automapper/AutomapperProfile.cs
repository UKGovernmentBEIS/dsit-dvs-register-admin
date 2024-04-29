using AutoMapper;
using DVSAdmin.BusinessLogic.Extensions;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PreRegistrationCountryMapping, PreRegistrationCountryMappingDto>();
            CreateMap<PreRegistrationCountryMappingDto, PreRegistrationCountryMapping>();
            CreateMap<Country, CountryDto>();
            CreateMap<CountryDto, Country>();
            CreateMap<PreRegistration, PreRegistrationDto>()
            .ForMember(dest => dest.PreRegistrationCountryMappings, opt => opt.MapFrom(src => src.PreRegistrationCountryMappings))
            .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftResolver>());

            CreateMap<PreRegistrationDto, PreRegistration>().ForMember(dest => dest.PreRegistrationCountryMappings, opt => opt.MapFrom(src => src.PreRegistrationCountryMappings))
            .ForMember(dest => dest.PreRegistrationReview, opt => opt.MapFrom(src => src.PreRegistrationReview));

            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            CreateMap<PreRegistrationReview, PreRegistrationReviewDto>();

            CreateMap<PreRegistrationReviewDto, PreRegistrationReview>();
            CreateMap<UniqueReferenceNumber, UniqueReferenceNumberDto>();

            CreateMap<UniqueReferenceNumberDto, UniqueReferenceNumber>();



        }
    }
}
