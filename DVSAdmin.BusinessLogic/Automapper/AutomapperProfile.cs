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


            //CreateMap<CertificateReviewRejectionReasonMappings, CertificateReviewRejectionReasonMappingsDto>();
            //CreateMap<CertificateReviewRejectionReasonMappingsDto, CertificateReviewRejectionReasonMappings>();
            //CreateMap<CertificateReview, CertificateReviewDto>().ForMember(dest => dest.CertificateReviewRejectionReasonMappings,
            //opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMappings));
            //CreateMap<CertificateReviewDto, CertificateReview>().ForMember(dest => dest.CertificateReviewRejectionReasonMappings,
            //opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMappings));

            //CreateMap<Provider, ProviderDto>().ForMember(dest => dest.CertificateInformation, opt => opt.MapFrom(src => src.CertificateInformation))
            //.ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftToPublishResolver>()); 
            //CreateMap<ProviderDto, Provider>().ForMember(dest => dest.CertificateInformation, opt => opt.MapFrom(src => src.CertificateInformation));

            CreateMap<ProviderProfile, ProviderProfileDto>()
           .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));
            CreateMap<ProviderProfileDto, ProviderProfile>()
            .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));


            CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.ServiceQualityLevelMapping, opt => opt.MapFrom(src => src.ServiceQualityLevelMapping))
            .ForMember(dest => dest.ServiceRoleMapping, opt => opt.MapFrom(src => src.ServiceRoleMapping))
            .ForMember(dest => dest.ServiceIdentityProfileMapping, opt => opt.MapFrom(src => src.ServiceIdentityProfileMapping))
            .ForMember(dest => dest.ServiceSupSchemeMapping, opt => opt.MapFrom(src => src.ServiceSupSchemeMapping))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider))
            .ForMember(dest => dest.CertificateReview, opt => opt.MapFrom(src => src.CertificateReview))
            .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftResolverCertificateReview>());

            CreateMap<ServiceDto, Service>()
           .ForMember(dest => dest.ServiceQualityLevelMapping, opt => opt.MapFrom(src => src.ServiceQualityLevelMapping))
           .ForMember(dest => dest.ServiceRoleMapping, opt => opt.MapFrom(src => src.ServiceRoleMapping))
           .ForMember(dest => dest.ServiceIdentityProfileMapping, opt => opt.MapFrom(src => src.ServiceIdentityProfileMapping))
           .ForMember(dest => dest.ServiceSupSchemeMapping, opt => opt.MapFrom(src => src.ServiceSupSchemeMapping))
           .ForMember(dest => dest.CertificateReview, opt => opt.MapFrom(src => src.CertificateReview))
           .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

        }
    }
}
