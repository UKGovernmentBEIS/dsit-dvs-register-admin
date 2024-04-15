using AutoMapper;
using DVSAdmin.BusinessLogic.Automapper;
using DVSAdmin.BusinessLogic.Extensions;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.PreRegistration;
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
            
            .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftResolver>())
            .ForMember(dest=>dest.PrimaryChecksCommentCount ,opt => opt.MapFrom<CommentsCountResolver>())
            .ForMember(dest => dest.SecondaryChecksCommentCount, opt => opt.MapFrom<CommentsCountResolver>());
            CreateMap<PreRegistrationDto, PreRegistration>().ForMember(dest => dest.PreRegistrationCountryMappings, opt => opt.MapFrom(src => src.PreRegistrationCountryMappings));
          


        }
    }
}
