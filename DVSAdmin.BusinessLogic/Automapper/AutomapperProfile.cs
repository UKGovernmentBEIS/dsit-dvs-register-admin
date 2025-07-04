﻿using AutoMapper;
using DVSAdmin.BusinessLogic.Automapper.Resolvers;
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

            CreateMap<ProviderProfileCabMapping, ProviderProfileCabMappingDto>();
            CreateMap<ProviderProfileCabMappingDto, ProviderProfileCabMapping>();


            CreateMap<ProviderProfile, ProviderProfileDto>()
           .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services))
           .ForMember(dest => dest.ProviderProfileCabMapping, opt => opt.MapFrom(src => src.ProviderProfileCabMapping))
           .ForMember(dest => dest.DaysLeftToComplete, opt => opt.MapFrom<DaysLeftToPublishResolver>());
            CreateMap<ProviderProfileDto, ProviderProfile>()
           .ForMember(dest => dest.ProviderProfileCabMapping, opt => opt.MapFrom(src => src.ProviderProfileCabMapping))
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
            .ForMember(dest => dest.DaysLeftToCompletePICheck, opt => opt.MapFrom<DaysLeftResolverPICheck>())
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom<CreatedTimeResolver>())
            .ForMember(dest => dest.NewOrResubmission, opt => opt.MapFrom<NewOrResubmissionResolver>());

            CreateMap<ServiceDto, Service>()
           .ForMember(dest => dest.ServiceQualityLevelMapping, opt => opt.MapFrom(src => src.ServiceQualityLevelMapping))
           .ForMember(dest => dest.ServiceRoleMapping, opt => opt.MapFrom(src => src.ServiceRoleMapping))
           .ForMember(dest => dest.ServiceIdentityProfileMapping, opt => opt.MapFrom(src => src.ServiceIdentityProfileMapping))
           .ForMember(dest => dest.ServiceSupSchemeMapping, opt => opt.MapFrom(src => src.ServiceSupSchemeMapping))
           .ForMember(dest => dest.CertificateReview, opt => opt.MapFrom(src => src.CertificateReview))
            .ForMember(dest => dest.PublicInterestCheck, opt => opt.MapFrom(src => src.PublicInterestCheck))
           .ForMember(dest => dest.CabUser, opt => opt.MapFrom(src => src.CabUser))
           .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));



            CreateMap<ServiceIdentityProfileMappingDraft, ServiceIdentityProfileMappingDraftDto>();
            CreateMap<ServiceIdentityProfileMappingDraftDto, ServiceIdentityProfileMappingDraft>().ForMember(x => x.IdentityProfile, opt => opt.Ignore());
            CreateMap<ServiceRoleMappingDraft, ServiceRoleMappingDraftDto>();
           CreateMap<ServiceRoleMappingDraftDto, ServiceRoleMappingDraft>().ForMember(x => x.Role, opt => opt.Ignore());


            CreateMap<ServiceSupSchemeMappingDraft, ServiceSupSchemeMappingDraftDto>();
            CreateMap<ServiceSupSchemeMappingDraftDto, ServiceSupSchemeMappingDraft>().ForMember(x => x.SupplementaryScheme, opt => opt.Ignore());
            CreateMap<ServiceQualityLevelMappingDraft, ServiceQualityLevelMappingDraftDto>();
            CreateMap<ServiceQualityLevelMappingDraftDto, ServiceQualityLevelMappingDraft>().ForMember(x => x.QualityLevel, opt => opt.Ignore());

            CreateMap<ServiceDraft, ServiceDraftDto>()
            .ForMember(dest => dest.ServiceQualityLevelMappingDraft, opt => opt.MapFrom(src => src.ServiceQualityLevelMappingDraft))
            .ForMember(dest => dest.ServiceRoleMappingDraft, opt => opt.MapFrom(src => src.ServiceRoleMappingDraft))
            .ForMember(dest => dest.ServiceIdentityProfileMappingDraft, opt => opt.MapFrom(src => src.ServiceIdentityProfileMappingDraft))
            .ForMember(dest => dest.ServiceSupSchemeMappingDraft, opt => opt.MapFrom(src => src.ServiceSupSchemeMappingDraft))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

            CreateMap<ServiceDraftDto, ServiceDraft>()
            .ForMember(dest => dest.ServiceQualityLevelMappingDraft, opt => opt.MapFrom(src => src.ServiceQualityLevelMappingDraft))
            .ForMember(dest => dest.ServiceRoleMappingDraft, opt => opt.MapFrom(src => src.ServiceRoleMappingDraft))
            .ForMember(dest => dest.ServiceIdentityProfileMappingDraft, opt => opt.MapFrom(src => src.ServiceIdentityProfileMappingDraft))
            .ForMember(dest => dest.ServiceSupSchemeMappingDraft, opt => opt.MapFrom(src => src.ServiceSupSchemeMappingDraft))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider));

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

            CreateMap<RequestManagement, RequestManagementDto>();
            CreateMap<RequestManagementDto, RequestManagement>();

            CreateMap<CabTransferRequest, CabTransferRequestDto>()
          .ForMember(dest => dest.RequestManagement, opt => opt.MapFrom(src => src.RequestManagement));
            CreateMap<CabTransferRequestDto, CabTransferRequest>()
            .ForMember(dest => dest.RequestManagement, opt => opt.MapFrom(src => src.RequestManagement));

            CreateMap<TrustFrameworkVersion, TrustFrameworkVersionDto>();
            CreateMap<TrustFrameworkVersionDto, TrustFrameworkVersion>();
            CreateMap<ManualUnderPinningService, ManualUnderPinningServiceDto>();
            CreateMap<ManualUnderPinningServiceDto, ManualUnderPinningService>();


        }
    }
}
