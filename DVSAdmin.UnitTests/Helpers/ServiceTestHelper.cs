using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.UnitTests
{
    internal static class ServiceTestHelper
    {
        public static CertificateReviewDto CreateCertificateReview(int verifiedUser, int serviceId, int providerId, CertificateReviewEnum certificateReviewStatus,
        string comments = "", string commentsForIncorrect = "", string rejectionComments = "", string amendments = "")
        {
            bool isPass = certificateReviewStatus == CertificateReviewEnum.Approved;
            CertificateReviewDto certificateReview = new()
            {

                ServiceId = serviceId,
                ProviProviderProfileId = providerId,
                IsCabLogoCorrect = isPass,
                IsCabDetailsCorrect = isPass,
                IsProviderDetailsCorrect = isPass,
                IsServiceNameCorrect = isPass,
                IsRolesCertifiedCorrect = isPass,
                IsCertificationScopeCorrect = isPass,
                IsServiceSummaryCorrect = isPass,
                IsURLLinkToServiceCorrect = isPass,
                IsGPG44Correct = isPass,
                IsGPG45Correct = isPass,
                IsServiceProvisionCorrect = isPass,
                IsLocationCorrect = isPass,
                IsDateOfIssueCorrect = isPass,
                IsDateOfExpiryCorrect = isPass,
                IsAuthenticyVerifiedCorrect = isPass,
                Comments = comments,
                InformationMatched = isPass,
                CommentsForIncorrect = commentsForIncorrect,
                RejectionComments = rejectionComments,
                Amendments = amendments,
                VerifiedUser = verifiedUser,
                CreatedDate = DateTime.Now,
                CertificateReviewStatus = certificateReviewStatus,
                CertificateReviewRejectionReasonMapping = certificateReviewStatus == CertificateReviewEnum.Rejected ?
                 new List<CertificateReviewRejectionReasonMappingDto>
                {
                    new() {CertificateReviewRejectionReasonId = 1 },
                    new() { CertificateReviewRejectionReasonId = 2 }
                } : null
            };

            return certificateReview;
        }
        public static ServiceDto CreateService(int cabUserId, string serviceName, int providerProfileId, ServiceStatusEnum serviceStatus,
            bool? hasGpg44, bool? hasGpg45, bool? hasSupplementarySchemes, int serviceKey)
        {
            var service = new ServiceDto
            {

                ServiceKey = serviceKey,
                ProviderProfileId = providerProfileId,
                ServiceName = serviceName,
                WebSiteAddress = "https://www.sample-service.com",
                CompanyAddress = "123 Sample St, Sample City, SC 12345",
                ServiceRoleMapping =
            [
                 new() { RoleId = 1},
                 new() { RoleId = 2 }
            ],
                HasGPG44 = hasGpg44,
                ServiceQualityLevelMapping = Convert.ToBoolean(hasGpg44) ? new List<ServiceQualityLevelMappingDto>
            {
                new() {  QualityLevelId = 1 },
                new() {  QualityLevelId = 4 }
            }
            : [],
                HasGPG45 = hasGpg45,
                ServiceIdentityProfileMapping = Convert.ToBoolean(hasGpg45) ? new List<ServiceIdentityProfileMappingDto>()
            {
             new() { IdentityProfileId = 1 } ,
             new() { IdentityProfileId = 2 }
            }
            : [],
                HasSupplementarySchemes = hasSupplementarySchemes,
                ServiceSupSchemeMapping = Convert.ToBoolean(hasSupplementarySchemes) ? new List<ServiceSupSchemeMappingDto>()
            {
             new() { SupplementarySchemeId = 1 } ,
             new() { SupplementarySchemeId = 2 }
            }
            : [],
                FileName = "sample-file.pdf",
                FileLink = "test.pdf",
                FileSizeInKb = 150.5m,
                ConformityIssueDate = DateTime.Now,
                ConformityExpiryDate = DateTime.Now.AddYears(2),
                CabUserId = cabUserId,
                ServiceStatus = serviceStatus

            };

            return service;
        }

        public static List<ServiceDto> CreateServiceCertificateReviewList()
        {
    
            List<ServiceDto> serviceDtos = [];

            var service1 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service1.Id = 1;
            serviceDtos.Add(service1);

            var service2 = CreateService(1, "Test service 2", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service2.Id = 2;
            service2.CertificateReview = CreateCertificateReview(1, 2, 1, CertificateReviewEnum.InReview, "approved comment", "page 2 comment", "", "");           
            serviceDtos.Add(service2);

            var service3 = CreateService(1, "Test service 3", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service3.Id = 3;
            service3.CertificateReview = CreateCertificateReview(1, 3, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");          
            serviceDtos.Add(service3);

            var service4 = CreateService(1, "Test service 4", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service4.Id = 4;
            service4.CertificateReview = CreateCertificateReview(1, 4, 1, CertificateReviewEnum.Rejected, "approved comment", "page 2 comment", "", "");          
            serviceDtos.Add(service4);
            return serviceDtos;
            
        }

        public static List<ServiceDto> CreateServiceSaveAsDraftAndRemovedList()
        {
          
            List<ServiceDto> serviceDtos = [];
            var service1 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.SavedAsDraft, true, true, true, 0);
            service1.Id = 1;
            serviceDtos.Add(service1);

            var service2 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Removed, true, true, true, 0);
            service2.Id = 2;
            service2.CertificateReview = CreateCertificateReview(1, 2, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");            
            serviceDtos.Add(service2);         
            return serviceDtos;

        }

        public static List<ServiceDto> CreateServiceAllStatusList()
        {

            List<ServiceDto> serviceDtos = [];
           
            var service1 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service1.Id = 1;
            serviceDtos.Add(service1);

            var service2 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Received, true, true, true, 0);
            service2.Id = 2;
            service2.CertificateReview = CreateCertificateReview(1, 2, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service2);

            var service3 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.ReadyToPublish, true, true, true, 0);
            service3.Id = 3;
            service3.CertificateReview = CreateCertificateReview(1, 3, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service3);

            var service4 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Published, true, true, true, 0);
            service4.Id = 4;
            service4.CertificateReview = CreateCertificateReview(1, 4, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service4);

            var service5 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Removed, true, true, true, 0);
            service5.Id = 5;
            service5.CertificateReview = CreateCertificateReview(1, 5, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service5);

            var service6 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.AwaitingRemovalConfirmation, true, true, true, 0);
            service6.Id = 6;
            service6.CertificateReview = CreateCertificateReview(1, 6, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service6);

            var service7 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.CabAwaitingRemovalConfirmation, true, true, true, 0);
            service7.Id = 7;
            service7.CertificateReview = CreateCertificateReview(1, 7, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service7);

            var service8 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.SavedAsDraft, true, true, true, 0);
            service8.Id = 8;
            serviceDtos.Add(service8);

            var service9 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.UpdatesRequested, true, true, true, 0);
            service9.Id = 9;
            service9.CertificateReview = CreateCertificateReview(1, 9, 1, CertificateReviewEnum.Approved, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service9);

            var service10 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.AmendmentsRequired, true, true, true, 0);
            service10.Id = 10;
            service10.CertificateReview = CreateCertificateReview(1, 10, 1, CertificateReviewEnum.AmendmentsRequired, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service10);

            var service11 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Resubmitted, true, true, true, 0);
            service11.Id = 11;
            serviceDtos.Add(service11);

            var service12 = CreateService(1, "Test service 1", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service12.Id = 12;
            service12.CertificateReview = CreateCertificateReview(1, 12, 1, CertificateReviewEnum.InReview, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service12);

            var service13= CreateService(1, "Test service 1", 1, ServiceStatusEnum.Submitted, true, true, true, 0);
            service13.Id = 13;
            service13.CertificateReview = CreateCertificateReview(1, 13, 1, CertificateReviewEnum.Rejected, "approved comment", "page 2 comment", "", "");
            serviceDtos.Add(service13);       

            return serviceDtos;

        }



    }
}
