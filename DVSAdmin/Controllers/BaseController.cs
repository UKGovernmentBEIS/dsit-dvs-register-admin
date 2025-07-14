using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class BaseController : Controller
    {
        public string UserEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;      
        public string UserProfile => HttpContext.Session.Get<string>("Profile") ?? string.Empty;

        protected string ControllerName => ControllerContext.ActionDescriptor.ControllerName;

        protected string ActionName => ControllerContext.ActionDescriptor.ActionName;

        public void SetRefererURL()
        {
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;
        }
        protected ServiceSummaryViewModel GetServiceSummary()
        {
            ServiceSummaryViewModel model = HttpContext?.Session.Get<ServiceSummaryViewModel>("ServiceSummary") ?? new ServiceSummaryViewModel
            {
                QualityLevelViewModel = new QualityLevelViewModel { SelectedLevelOfProtections = new List<QualityLevelDto>(), SelectedQualityofAuthenticators = new List<QualityLevelDto>() },
                RoleViewModel = new RoleViewModel { SelectedRoles = new List<RoleDto>() },
                IdentityProfileViewModel = new IdentityProfileViewModel { SelectedIdentityProfiles = new List<IdentityProfileDto>() },
                SupplementarySchemeViewModel = new SupplementarySchemeViewModel { SelectedSupplementarySchemes = new List<SupplementarySchemeDto> { } }
            };
            return model;
        }        

        protected IActionResult RedirectToMissingMappings(bool fromSummaryPage)
        {
            int nextMissingSchemeIdForGpg45 = NextMissingSchemId("GPG45");
            int nextMissingSchemeIdForGpg44 = NextMissingSchemId("GPG44");
            if (nextMissingSchemeIdForGpg45 > 0)
                return RedirectToAction("SchemeGPG45", "EditServiceTrustFramework0_4", new { fromSummaryPage = fromSummaryPage, schemeId = nextMissingSchemeIdForGpg45 });
            else if (nextMissingSchemeIdForGpg44 > 0)
                return RedirectToAction("SchemeGPG44Input", "EditServiceTrustFramework0_4", new { fromSummaryPage = fromSummaryPage, schemeId = nextMissingSchemeIdForGpg44 });
            else
                return RedirectToAction("ServiceSummary", "EditService");
        }
        private int NextMissingSchemId(string type)
        {
            int missingSchemeId = 0;
            var serviceSummary = GetServiceSummary();
            List<int> selectedSchemeIds = serviceSummary.SupplementarySchemeViewModel.SelectedSupplementarySchemes.OrderBy(x => x.Id).Select(x => x.Id).ToList();
            List<int> existingSchemeIds = [];
            if (type == "GPG45")
            {
                existingSchemeIds = serviceSummary.SchemeIdentityProfileMapping?.OrderBy(x => x.SchemeId).Select(mapping => mapping.SchemeId).ToList() ?? [];
            }
            else if (type == "GPG44")
            {
                existingSchemeIds = serviceSummary.SchemeQualityLevelMapping?.OrderBy(x => x.SchemeId).Select(mapping => mapping.SchemeId).ToList() ?? [];
            }


            if (selectedSchemeIds != null && existingSchemeIds != null)
            {
                var missingSchemeIds = selectedSchemeIds.Except(existingSchemeIds).ToList();

                if (missingSchemeIds.Any())
                {
                    missingSchemeId = missingSchemeIds[0];
                }
            }
            return missingSchemeId;

        }


    }
}
