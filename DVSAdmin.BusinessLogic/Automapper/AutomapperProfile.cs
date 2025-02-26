using AutoMapper;
using DVSAdmin.BusinessLogic.Extensions;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {        

            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();       

            CreateMap<Role, RoleDto>();
            CreateMap<RoleDto, Role>();
            CreateMap<IdentityProfile, IdentityProfileDto>();
            CreateMap<IdentityProfileDto, IdentityProfile>();
            CreateMap<SupplementaryScheme, SupplementarySchemeDto>();
            CreateMap<SupplementarySchemeDto, SupplementaryScheme>();         
            CreateMap<CertificateReviewRejectionReason, CertificateReviewRejectionReasonDto>();
            CreateMap<CertificateReviewRejectionReasonDto, CertificateReviewRejectionReason>();

            CreateMap<QualityLevel, QualityLevelDto>();
            CreateMap<QualityLevelDto, QualityLevel>();

            CreateMap<ServiceIdentityProfileMapping, ServiceIdentityProfileMappingDto>();
            CreateMap<ServiceIdentityProfileMappingDto, ServiceIdentityProfileMapping>();
            CreateMap<ServiceRoleMapping, ServiceRoleMappingDto>();
            CreateMap<ServiceRoleMappingDto, ServiceRoleMapping>();
            CreateMap<ServiceSupSchemeMapping, ServiceSupSchemeMappingDto>();
            CreateMap<ServiceSupSchemeMappingDto, ServiceSupSchemeMapping>();
            CreateMap<ServiceQualityLevelMapping, ServiceQualityLevelMappingDto>();
            CreateMap<ServiceQualityLevelMappingDto, ServiceQualityLevelMapping>();



            CreateMap<CertificateReviewRejectionReasonMapping, CertificateReviewRejectionReasonMappingDto>();
            CreateMap<CertificateReviewRejectionReasonMappingDto, CertificateReviewRejectionReasonMapping>();
            CreateMap<CertificateReview, CertificateReviewDto>().ForMember(dest => dest.CertificateReviewRejectionReasonMapping,
            opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMapping));
            CreateMap<CertificateReviewDto, CertificateReview>().ForMember(dest => dest.CertificateReviewRejectionReasonMapping,
            opt => opt.MapFrom(src => src.CertificateReviewRejectionReasonMapping));


            CreateMap<ProviderProfile, ProviderProfileDto>()
           .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services))
           .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftToPublishResolver>());
            CreateMap<ProviderProfileDto, ProviderProfile>()
           .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));
            CreateMap<ProviderProfileDraft, ProviderProfileDraftDto>()
           .ReverseMap();
            
            CreateMap<Cab, CabDto>();
            CreateMap<CabDto, Cab>();
            CreateMap<CabUser, CabUserDto>();
            CreateMap<CabUserDto, CabUser>();

            CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.ServiceQualityLevelMapping, opt => opt.MapFrom(src => src.ServiceQualityLevelMapping))
            .ForMember(dest => dest.ServiceRoleMapping, opt => opt.MapFrom(src => src.ServiceRoleMapping))
            .ForMember(dest => dest.ServiceIdentityProfileMapping, opt => opt.MapFrom(src => src.ServiceIdentityProfileMapping))
            .ForMember(dest => dest.ServiceSupSchemeMapping, opt => opt.MapFrom(src => src.ServiceSupSchemeMapping))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider))
            .ForMember(dest => dest.CertificateReview, opt => opt.MapFrom(src => src.CertificateReview))
            .ForMember(dest => dest.PublicInterestCheck, opt => opt.MapFrom(src => src.PublicInterestCheck))
            .ForMember(dest => dest.CabUser, opt => opt.MapFrom(src => src.CabUser))
            .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftResolverCertificateReview>())
            .ForMember(dest => dest.DaysLeftToCompletePICheck, opt => opt.MapFrom<DaysLeftResolverPICheck>());

            CreateMap<ServiceDto, Service>()
           .ForMember(dest => dest.ServiceQualityLevelMapping, opt => opt.MapFrom(src => src.ServiceQualityLevelMapping))
           .ForMember(dest => dest.ServiceRoleMapping, opt => opt.MapFrom(src => src.ServiceRoleMapping))
           .ForMember(dest => dest.ServiceIdentityProfileMapping, opt => opt.MapFrom(src => src.ServiceIdentityProfileMapping))
           .ForMember(dest => dest.ServiceSupSchemeMapping, opt => opt.MapFrom(src => src.ServiceSupSchemeMapping))
           .ForMember(dest => dest.CertificateReview, opt => opt.MapFrom(src => src.CertificateReview))
            .ForMember(dest => dest.PublicInterestCheck, opt => opt.MapFrom(src => src.PublicInterestCheck))
           .ForMember(dest => dest.CabUser, opt => opt.MapFrom(src => src.CabUser))
           .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

            CreateMap<ServiceDraft, ServiceDraftDto>().ReverseMap();
            
            CreateMap<PublicInterestCheck, PublicInterestCheckDto>();
            CreateMap<PublicInterestCheckDto, PublicInterestCheck>();

            CreateMap<ProceedApplicationConsentToken, ProceedApplicationConsentTokenDto>()
           .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));
            CreateMap<ProceedApplicationConsentTokenDto, ProceedApplicationConsentToken>()
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));

            CreateMap<ProceedPublishConsentToken, ProceedPublishConsentTokenDto>()
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));
            CreateMap<ProceedPublishConsentTokenDto, ProceedPublishConsentToken>()
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));
            

        }
    }
}
