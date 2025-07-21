using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.TrustFramework;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models.Edit;

namespace DVSAdmin.Models
{
    public class ViewModelHelper
    {
        public static List<string> GetCabsForProvider(List<ServiceDto> services)
        {
            List<string> disctinctCabs = services.Select(s => s.CabUser.Cab.CabName).Distinct().ToList();

            if(disctinctCabs!=null && disctinctCabs.Count>1  )
            {
                if (!services.All(s => s.ServiceStatus == ServiceStatusEnum.Removed))
                {
                    disctinctCabs = services.Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                  s.ServiceStatus == ServiceStatusEnum.Published ||
                  s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                  s.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign ||
                  s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation ||
                  s.ServiceStatus == ServiceStatusEnum.UpdatesRequested)
                 .Select(s => s.CabUser.Cab.CabName).Distinct().ToList();
                }

                  
            }


            return disctinctCabs??[];
        }

        public static string GetServiceType(ServiceTypeEnum? input)
        {
            if (input == ServiceTypeEnum.UnderPinning) return "Underpinning";
            else if (input == ServiceTypeEnum.WhiteLabelled) return "White-labelled";
            else return "Neither";


        }
        public static DateViewModel GetDayMonthYear(DateTime? dateTime)
        {
            DateViewModel dateViewModel = new();
            DateTime conformityIssueDate = Convert.ToDateTime(dateTime);
            dateViewModel.Day = conformityIssueDate.Day;
            dateViewModel.Month = conformityIssueDate.Month;
            dateViewModel.Year = conformityIssueDate.Year;
            return dateViewModel;
        }
        public static void ClearUnderPinningServiceFieldsBeforeManualEntry(ServiceSummaryViewModel summaryViewModel)
        {
            if (summaryViewModel.ServiceType == CommonUtility.Models.Enums.ServiceTypeEnum.WhiteLabelled)
            {
                summaryViewModel.SelectedManualUnderPinningServiceId = null;
                summaryViewModel.SelectedUnderPinningServiceId = null;
                summaryViewModel.UnderPinningServiceName = null;
                summaryViewModel.UnderPinningProviderName = null;
                summaryViewModel.SelectCabViewModel = null;
                summaryViewModel.UnderPinningServiceExpiryDate = null;
            }
        }

        public static ServiceDraftDto MapToDraft(ServiceDto existingService, ServiceSummaryViewModel updatedService)
        {
            var draft = new ServiceDraftDto
            {
                serviceId = existingService.Id,
                PreviousServiceStatus = existingService.ServiceStatus,
                ProviderProfileId = existingService.ProviderProfileId
            };

            MapServiceFields(existingService, updatedService, draft);

            if (existingService.TrustFrameworkVersion.Version == Constants.TFVersion0_4)
            {
                MapTFVersion0_4SchemeMappingFields(existingService, updatedService, draft);

                MapTFVersion0_4UnderpinningServiceFields(existingService, updatedService, draft);
            }



            return (draft);
        }

        private static void MapServiceFields(ServiceDto existingService, ServiceSummaryViewModel updatedService, ServiceDraftDto draft)
        {
            var existingRoleIds = existingService.ServiceRoleMapping.Select(m => m.RoleId).ToList();
            var updatedRoleIds = updatedService.RoleViewModel.SelectedRoles.Select(m => m.Id).ToList(); ;

            var existingProtectionIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Protection)
                .Select(item => item.QualityLevelId);
            var updatedProtectionIds = updatedService.QualityLevelViewModel.SelectedLevelOfProtections
                .Select(item => item.Id);

            var existingAuthenticationIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                .Select(item => item.QualityLevelId);
            var updatedAuthenticationIds = updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators
                .Select(item => item.Id);

            var existingIdentityProfileIds = existingService.ServiceIdentityProfileMapping.Select(m => m.IdentityProfileId).ToList();
            var updatedIdentityProfileIds = updatedService.IdentityProfileViewModel.SelectedIdentityProfiles.Select(m => m.Id).ToList();

            var existingSupSchemeIds = existingService.ServiceSupSchemeMapping.Select(m => m.SupplementarySchemeId).ToList();
            var updatedSupSchemeIds = updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes.Select(m => m.Id).ToList();


            if (existingService.ServiceName != updatedService.ServiceName)
            {
                draft.ServiceName = updatedService.ServiceName;
            }

            if (existingService.CompanyAddress != updatedService.CompanyAddress)
            {
                draft.CompanyAddress = updatedService.CompanyAddress;
            }

            if (existingService.HasGPG44 != updatedService.HasGPG44)
            {
                draft.HasGPG44 = updatedService.HasGPG44;
            }

            if (existingService.HasGPG45 != updatedService.HasGPG45)
            {
                draft.HasGPG45 = updatedService.HasGPG45;
            }

            if (existingService.HasSupplementarySchemes != updatedService.HasSupplementarySchemes)
            {
                draft.HasSupplementarySchemes = updatedService.HasSupplementarySchemes;
            }

            if (existingService.ConformityIssueDate != updatedService.ConformityIssueDate)
            {
                draft.ConformityIssueDate = updatedService.ConformityIssueDate;
            }

            if (existingService.ConformityExpiryDate != updatedService.ConformityExpiryDate)
            {
                draft.ConformityExpiryDate = updatedService.ConformityExpiryDate;
            }

            if (!existingRoleIds.OrderBy(id => id).SequenceEqual(updatedRoleIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.RoleViewModel.SelectedRoles)
                {
                    draft.ServiceRoleMappingDraft.Add(new ServiceRoleMappingDraftDto { RoleId = item.Id, Role = item });

                }
            }
            if (!existingProtectionIds.OrderBy(id => id).SequenceEqual(updatedProtectionIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id, QualityLevel = item });
                }
            }

            if (!existingAuthenticationIds.OrderBy(id => id).SequenceEqual(updatedAuthenticationIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id, QualityLevel = item });
                }
            }

            if (!existingIdentityProfileIds.OrderBy(id => id).SequenceEqual(updatedIdentityProfileIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.IdentityProfileViewModel.SelectedIdentityProfiles)
                {
                    draft.ServiceIdentityProfileMappingDraft.Add(new ServiceIdentityProfileMappingDraftDto { IdentityProfileId = item.Id, IdentityProfile = item });
                }
            }

            if (!existingSupSchemeIds.OrderBy(id => id).SequenceEqual(updatedSupSchemeIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes)
                {
                    draft.ServiceSupSchemeMappingDraft.Add(new ServiceSupSchemeMappingDraftDto { SupplementarySchemeId = item.Id, SupplementaryScheme = item });
                }
            }
        }

        private static void MapTFVersion0_4SchemeMappingFields(ServiceDto existingService, ServiceSummaryViewModel updatedService, ServiceDraftDto draft)
        {
            Dictionary<int, List<IdentityProfileDto>>? existingSchemeIdentityProfileMappings = [];
            Dictionary<int, List<IdentityProfileDto>>? updatedSchemeIdentityProfileMappings = [];

            if (updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes != null && updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes.Count > 0)
            {
               
                foreach (var schemeMapping in existingService.ServiceSupSchemeMapping)
                {
                    existingSchemeIdentityProfileMappings.Add(schemeMapping.SupplementarySchemeId, schemeMapping.SchemeGPG45Mapping.Select(m => m.IdentityProfile).ToList());
                }

                foreach (var schemeMapping in updatedService.SchemeIdentityProfileMapping)
                {
                    updatedSchemeIdentityProfileMappings.Add(schemeMapping.SchemeId, schemeMapping.IdentityProfile.SelectedIdentityProfiles);
                }


                Dictionary<int, QualityLevelDtoWithFlagDto>? existingSchemeQualityLevelMappings = new();
                Dictionary<int, QualityLevelDtoWithFlagDto>? updatedSchemeQualityLevelMappings = [];


                foreach (var schemeMapping in existingService.ServiceSupSchemeMapping)
                {

                    QualityLevelDtoWithFlagDto qualityLevelDtoWithFlagDto = new();
                    qualityLevelDtoWithFlagDto.HasGpg44 = schemeMapping.HasGpg44Mapping;
                    if (schemeMapping.HasGpg44Mapping == true)
                    {
                        qualityLevelDtoWithFlagDto.QualityLevels = schemeMapping.SchemeGPG44Mapping.Select(x => x.QualityLevel).ToList();
                    }

                    existingSchemeQualityLevelMappings.Add(schemeMapping.SupplementarySchemeId, qualityLevelDtoWithFlagDto);
                }

                foreach (var schemeMapping in updatedService.SchemeQualityLevelMapping)
                {
                    QualityLevelDtoWithFlagDto qualityLevelDtoWithFlagDto = new();
                    qualityLevelDtoWithFlagDto.HasGpg44 = schemeMapping.HasGPG44;
                    if (schemeMapping.HasGPG44 == true)
                    {
                        qualityLevelDtoWithFlagDto.QualityLevels = schemeMapping.QualityLevel.SelectedLevelOfProtections.Union(schemeMapping.QualityLevel.SelectedQualityofAuthenticators).ToList();
                    }
                    updatedSchemeQualityLevelMappings.Add(schemeMapping.SchemeId, qualityLevelDtoWithFlagDto);
                }


                List<SchemeGPG45MappingDraftDto> gpg45MappingDraft;
                List<SchemeGPG44MappingDraftDto> gpg44MappingDraft;

                
                    foreach (var schemeMapping in updatedSchemeIdentityProfileMappings)
                    {
                        gpg45MappingDraft = [];
                        if (!existingSchemeIdentityProfileMappings.ContainsKey(schemeMapping.Key))
                        {
                            if (updatedSchemeIdentityProfileMappings.TryGetValue(schemeMapping.Key, out var updatedList))
                            {
                                foreach (var item in updatedList)
                                {
                                    gpg45MappingDraft.Add(new SchemeGPG45MappingDraftDto { IdentityProfileId = item.Id, IdentityProfile = item });
                                }
                            }
                        }
                        else
                        {
                            // Existing scheme, check for value changes
                            if (updatedSchemeIdentityProfileMappings.TryGetValue(schemeMapping.Key, out var updatedList))
                            {
                                var existingIds = existingSchemeIdentityProfileMappings[schemeMapping.Key].OrderBy(x => x.Id).Select(x => x.Id).ToList();
                                var updatedIds = updatedList.OrderBy(x => x.Id).Select(x => x.Id).ToList();
                                if (!existingIds.SequenceEqual(updatedIds))
                                {
                                    foreach (var item in updatedList)
                                    {
                                        gpg45MappingDraft.Add(new SchemeGPG45MappingDraftDto { IdentityProfileId = item.Id, IdentityProfile = item });
                                    }
                                }
                            }
                        }


                        ServiceSupSchemeMappingDraftDto serviceSupSchemeMappingDraft = draft.ServiceSupSchemeMappingDraft.Where(x => x.SupplementarySchemeId == schemeMapping.Key).FirstOrDefault()!;

                        // Selected schemes not changed, only mapping changes, so add entry to serviceSupSchemeMappingDraft
                        if (serviceSupSchemeMappingDraft == null)
                        {
                            SupplementarySchemeDto supplementaryScheme = existingService.ServiceSupSchemeMapping.Where(mapping => mapping.SupplementarySchemeId == schemeMapping.Key).
                            Select(mapping => mapping.SupplementaryScheme).FirstOrDefault()!;
                            draft.ServiceSupSchemeMappingDraft.Add(new ServiceSupSchemeMappingDraftDto
                            {
                                SupplementarySchemeId = schemeMapping.Key,
                                SchemeGPG45MappingDraft = gpg45MappingDraft,
                                SupplementaryScheme = supplementaryScheme
                            });
                        }
                        else
                        {
                            serviceSupSchemeMappingDraft.SchemeGPG45MappingDraft = gpg45MappingDraft;
                        }

                    }


                    foreach (var schemeMapping in updatedSchemeQualityLevelMappings)
                    {
                        gpg44MappingDraft = [];
                        if (!existingSchemeQualityLevelMappings.ContainsKey(schemeMapping.Key)) // new schemes added
                        {
                            if (updatedSchemeQualityLevelMappings.TryGetValue(schemeMapping.Key, out var updatedList))
                            {
                                if (updatedList.HasGpg44 == true && updatedList.QualityLevels != null)
                                {
                                    foreach (var item in updatedList.QualityLevels)
                                    {
                                        gpg44MappingDraft.Add(new SchemeGPG44MappingDraftDto { QualityLevelId = item.Id, QualityLevel = item });
                                    }
                                }

                            }
                        }


                        else
                        {
                            // Existing key, check for value changes
                            if (updatedSchemeQualityLevelMappings.TryGetValue(schemeMapping.Key, out var updatedList))
                            {
                                var existingIds = existingSchemeQualityLevelMappings[schemeMapping.Key].QualityLevels?.OrderBy(x => x.Id).Select(x => x.Id).ToList();
                                var updatedIds = updatedList?.QualityLevels?.OrderBy(x => x.Id).Select(x => x.Id).ToList();
                                if ( existingIds ==null ||(   existingIds != null && updatedIds != null && !existingIds.SequenceEqual(updatedIds)))
                                {
                                    if (updatedList?.QualityLevels != null)
                                    {
                                        foreach (var item in updatedList.QualityLevels)
                                        {
                                            gpg44MappingDraft.Add(new SchemeGPG44MappingDraftDto { QualityLevelId = item.Id, QualityLevel = item });
                                        }
                                    }
                                }
                            }
                        }

                        // Update the ServiceSupSchemeMappingDraftDto if there are changes
                        ServiceSupSchemeMappingDraftDto serviceSupSchemeMappingDraft = draft.ServiceSupSchemeMappingDraft.Where(x => x.SupplementarySchemeId == schemeMapping.Key).FirstOrDefault()!;
                        if (serviceSupSchemeMappingDraft == null )
                        {
                            SupplementarySchemeDto supplementaryScheme = existingService.ServiceSupSchemeMapping.Where(mapping => mapping.SupplementarySchemeId == schemeMapping.Key).
                            Select(mapping => mapping.SupplementaryScheme).FirstOrDefault()!;
                            draft.ServiceSupSchemeMappingDraft.Add(new ServiceSupSchemeMappingDraftDto
                            {
                                SupplementarySchemeId = schemeMapping.Key,
                                HasGpg44Mapping = schemeMapping.Value.HasGpg44,
                                SchemeGPG44MappingDraft = gpg44MappingDraft,
                                SupplementaryScheme = supplementaryScheme
                            });
                        }
                        else
                        {
                            serviceSupSchemeMappingDraft.HasGpg44Mapping = schemeMapping.Value.HasGpg44;
                            serviceSupSchemeMappingDraft.SchemeGPG44MappingDraft = gpg44MappingDraft;
                        }

                    }              
               



            }
        }

        private static void MapTFVersion0_4UnderpinningServiceFields(ServiceDto existingService, ServiceSummaryViewModel updatedService, ServiceDraftDto draft)
        {


            if (existingService.IsUnderPinningServicePublished == true && existingService.UnderPinningServiceId != null) // previous selected service was under pinning published
            {
                draft.IsUnderpinningServicePublished = updatedService.IsUnderpinningServicePublished;
                if (existingService.IsUnderPinningServicePublished == true && updatedService.IsUnderpinningServicePublished == true 
                    && updatedService.SelectedUnderPinningServiceId != null &&  updatedService.SelectedUnderPinningServiceId != existingService.UnderPinningServiceId)//current data is selected from list of manual services 
                {
                    draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.PublishedToPublished;                   
                    draft.UnderPinningServiceId = updatedService.SelectedUnderPinningServiceId;

                }
                else if (updatedService.IsUnderpinningServicePublished == false && updatedService.SelectedManualUnderPinningServiceId != null)//current data is selected from list of manual services 
                {
                    draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.PublishedToSelectOrChangeManual;
                    UpdateDraftWithManualUnderPinningServiceChanges(updatedService, draft);

                }
                else if (updatedService.IsUnderpinningServicePublished == false && updatedService.SelectedManualUnderPinningServiceId == null) // means manually entered 
                {
                    draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.PublishedToEnterManual;
                    UpdateDraftWithManualServiceDetails(updatedService, draft);
                }
            }
            else if (existingService.IsUnderPinningServicePublished == false && existingService.ManualUnderPinningServiceId != null) // previous selected manual service
            {
                draft.IsUnderpinningServicePublished = updatedService.IsUnderpinningServicePublished;
                if (updatedService.IsUnderpinningServicePublished == true && updatedService.SelectedUnderPinningServiceId != null)
                {
                    draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.ManualToPublished;               
                    draft.UnderPinningServiceId = updatedService.SelectedUnderPinningServiceId;
                }
                else if (existingService.IsUnderPinningServicePublished == false && updatedService.SelectedManualUnderPinningServiceId != null)//  selected service
                {
                   if(updatedService.SelectedManualUnderPinningServiceId!=existingService.ManualUnderPinningServiceId 
                    || updatedService.UnderPinningServiceName!= existingService.ManualUnderPinningService.ServiceName
                        || updatedService.UnderPinningProviderName != existingService.ManualUnderPinningService.ProviderName
                        || updatedService.SelectCabViewModel.SelectedCabId != existingService.ManualUnderPinningService.CabId
                        || updatedService.UnderPinningServiceExpiryDate != existingService.ManualUnderPinningService.CertificateExpiryDate)
                    {
                        draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.ManualToSelectOrChangeManual;
                        UpdateDraftWithManualUnderPinningServiceChanges(updatedService, draft);
                    }
                   
                }
                else if (existingService.IsUnderPinningServicePublished == false && updatedService.SelectedManualUnderPinningServiceId == null) // manually entered service
                {
                    draft.UnderPinninngServiceEditType = UnderPinninngServiceEditEnum.ManualToEnterManual;
                    UpdateDraftWithManualServiceDetails(updatedService, draft);
                }
            }





        }

        private static void UpdateDraftWithManualUnderPinningServiceChanges(ServiceSummaryViewModel updatedService, ServiceDraftDto draft)
        {
          
            draft.ManualUnderPinningService = new();
            draft.ManualUnderPinningServiceId = updatedService.SelectedManualUnderPinningServiceId;

            draft.ManualUnderPinningServiceDraft = new();

            draft.ManualUnderPinningServiceDraft.ServiceName = updatedService.UnderPinningServiceName;
            draft.ManualUnderPinningServiceDraft.ProviderName = updatedService.UnderPinningProviderName;
            draft.ManualUnderPinningServiceDraft.CertificateExpiryDate = updatedService.UnderPinningServiceExpiryDate;
            draft.ManualUnderPinningServiceDraft.SelectedCabName = updatedService.SelectCabViewModel.SelectedCabName;
            draft.ManualUnderPinningServiceDraft.CabId = updatedService.SelectCabViewModel.SelectedCabId;
        }

        private static void UpdateDraftWithManualServiceDetails(ServiceSummaryViewModel updatedService, ServiceDraftDto draft)
        {
            draft.IsUnderpinningServicePublished = updatedService.IsUnderpinningServicePublished;
            draft.ManualUnderPinningServiceDraft = new()
            {
                ServiceName = updatedService.UnderPinningServiceName,
                ProviderName = updatedService.UnderPinningProviderName,
                SelectedCabName = updatedService.SelectCabViewModel.SelectedCabName,
                CabId = updatedService.SelectCabViewModel.SelectedCabId,
                CertificateExpiryDate = updatedService.UnderPinningServiceExpiryDate
            };
        }
    }
}
