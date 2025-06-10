using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.Controllers;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace DVSAdmin.UnitTests.DVSAdmin
{
    public class CertificateReviewControllerTests
    {
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IUserService userService;
        private readonly IBucketService bucketService;
        private readonly IConfiguration configuration;
        private readonly CertificateReviewController certificateReviewController;

        public CertificateReviewControllerTests()
        {
            certificateReviewService = Substitute.For<ICertificateReviewService>();
            userService = Substitute.For<IUserService>();
            bucketService = Substitute.For<IBucketService>();
            configuration = Substitute.For<IConfiguration>();
            configuration["Key"].Returns("Value");
            certificateReviewController = new CertificateReviewController(certificateReviewService, userService, bucketService, configuration); 


        }

        [Fact]
        public async Task CertificateReviews_ReturnsViewResult_WithValidData()
        {
            var serviceList = ServiceTestHelper.CreateServiceCertificateReviewList();
            certificateReviewService.GetServiceList().Returns(serviceList);
            var result = await certificateReviewController.CertificateReviews();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateReviewListViewModel>(viewResult.ViewData.Model);
            Assert.Equal(2, model?.CertificateReviewList?.Count); 
            Assert.Equal(2, model?.ArchiveList?.Count); 
        }

        [Fact]
        public async Task CertificateReviews_ReturnsViewResult_WithNoRecords()
        {
            
            var serviceList = ServiceTestHelper.CreateServiceSaveAsDraftAndRemovedList();
            certificateReviewService.GetServiceList().Returns(serviceList);            
            var result = await certificateReviewController.CertificateReviews();            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateReviewListViewModel>(viewResult.ViewData.Model);
            Assert.Equal(0, model?.CertificateReviewList?.Count);
            Assert.Equal(1, model?.ArchiveList?.Count);
        }

        [Fact]
        public async Task CertificateReviews_ReturnsViewResult_WithValidData_ForAllStatuses()
        {
            var serviceList = ServiceTestHelper.CreateServiceAllStatusList();
            certificateReviewService.GetServiceList().Returns(serviceList);
            var result = await certificateReviewController.CertificateReviews();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateReviewListViewModel>(viewResult.ViewData.Model);
            Assert.Equal(4, model?.CertificateReviewList?.Count);
            Assert.Equal(8, model?.ArchiveList?.Count);
        }


    }
}
