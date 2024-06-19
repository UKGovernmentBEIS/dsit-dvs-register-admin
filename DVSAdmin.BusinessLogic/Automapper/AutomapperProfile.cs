using AutoMapper;
using DVSAdmin.BusinessLogic.Automapper;
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
            CreateMap<UniqueReferenceNumber, UniqueReferenceNumberDto>()
            .ForMember(dest => dest.URNStatus, opt => opt.MapFrom<URNStatusResolver>());

            CreateMap<UniqueReferenceNumberDto, UniqueReferenceNumber>();

            CreateMap<Role, RoleDto>();
            CreateMap<RoleDto, Role>();
            CreateMap<IdentityProfile, IdentityProfileDto>();
            CreateMap<IdentityProfileDto, IdentityProfile>();
            CreateMap<SupplementaryScheme, SupplementarySchemeDto>();
            CreateMap<SupplementarySchemeDto, SupplementaryScheme>();
            CreateMap<CertificateInfoIdentityProfileMapping, CertificateInfoIdentityProfileMappingDto>();
            CreateMap<CertificateInfoIdentityProfileMappingDto, CertificateInfoIdentityProfileMapping>();
            CreateMap<CertificateInfoRoleMapping, CertificateInfoRoleMappingDto>();
            CreateMap<CertificateInfoRoleMappingDto, CertificateInfoRoleMapping>();
            CreateMap<CertificateInfoSupSchemeMapping, CertificateInfoSupSchemeMappingDto>();
            CreateMap<CertificateInfoSupSchemeMappingDto, CertificateInfoSupSchemeMapping>();
            CreateMap<CertificateReviewRejectionReason, CertificateReviewRejectionReasonDto>();
            CreateMap<CertificateReviewRejectionReasonDto, CertificateReviewRejectionReason>();


            CreateMap<CertificateInformation, CertificateInformationDto>()
            .ForMember(dest => dest.CertificateInfoRoleMapping, opt => opt.MapFrom(src => src.CertificateInfoRoleMapping))
            .ForMember(dest => dest.CertificateInfoIdentityProfileMapping, opt => opt.MapFrom(src => src.CertificateInfoIdentityProfileMapping))
            .ForMember(dest => dest.CertificateInfoSupSchemeMappings, opt => opt.MapFrom(src => src.CertificateInfoSupSchemeMappings))
            .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftResolverCertificateReview>())
            .ForMember(dest => dest.CertificateInfoStatus, opt => opt.MapFrom<CertificateReviewStatusResolver>())
             .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

            CreateMap<CertificateInformationDto, CertificateInformation>()
           .ForMember(dest => dest.CertificateInfoRoleMapping, opt => opt.MapFrom(src => src.CertificateInfoRoleMapping))
           .ForMember(dest => dest.CertificateInfoIdentityProfileMapping, opt => opt.MapFrom(src => src.CertificateInfoIdentityProfileMapping))
           .ForMember(dest => dest.CertificateInfoSupSchemeMappings, opt => opt.MapFrom(src => src.CertificateInfoSupSchemeMappings))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

            CreateMap<CertificateReviewRejectionReasonMappings, CertificateReviewRejectionReasonMappingsDto>();
            CreateMap<CertificateReviewRejectionReasonMappingsDto, CertificateReviewRejectionReasonMappings>();
            CreateMap<CertificateReview, CertificateReviewDto>().ForMember(dest => dest.CertificateReviewRejectionReasonMappings,
            opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMappings));
            CreateMap<CertificateReviewDto, CertificateReview>().ForMember(dest => dest.CertificateReviewRejectionReasonMappings,
            opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMappings));

            CreateMap<Provider, ProviderDto>();
            CreateMap<ProviderDto, Provider>();
           


        }
    }
}
