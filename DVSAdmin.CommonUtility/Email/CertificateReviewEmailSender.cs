using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class CertificateReviewEmailSender : EmailSender
    {
        public CertificateReviewEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }


        #region Certificate Review

        public async Task<bool> SendCertificateInfoApprovedToCab(string recipientName, string companyName, string serviceName, string emailAddress)
        {
            var template = govUkNotifyConfig.CertificateInfoApprovedToCabTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName}

            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendCertificateInfoApprovedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.CertificateInfoApprovedToDSIT;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName}

            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendCertificateInfoRejectedToCab(string recipientName, string companyName, string serviceName, string rejectionCategory, string rejectionComments, string emailAddress)
        {
            var template = govUkNotifyConfig.CertificateInfoRejectedToCabTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.RejectionCategory,  rejectionCategory},
                { template.RejectionComments,  rejectionComments}

            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendCertificateInfoRejectedToDSIT(string companyName, string serviceName, string rejectionCategory, string rejectionComments)
        {
            var template = govUkNotifyConfig.CertificateInfoRejectedToDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.RejectionCategory,  rejectionCategory},
                { template.RejectionComments,  rejectionComments},


             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendApplicationRestroredToDSIT()
        {
            var template = govUkNotifyConfig.CertificateReviewRestoredToDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.LoginLink,  govUkNotifyConfig.LoginLink}
             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendCertificateBackToCab(string recipientName, string companyName, string serviceName, string emailAddress, string amendmentsNeeded)
        {
            var template = govUkNotifyConfig.CertificateSentBackToCabTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.ResubmissionFeedback,  amendmentsNeeded}

            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendCertificateBackDSIT(string companyName, string serviceName, string amendmentsNeeded)
        {
            var template = govUkNotifyConfig.CertificateSentBackDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.ResubmissionFeedback,  amendmentsNeeded}

            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }


        #endregion


        #region Opening the loop
        public async Task<bool> SendProceedApplicationConsentToDIP(string companyName, string serviceName, string companyNumber, string companyAddress, string publicContactEmail,
          string publicPhoneNumber, string consentLink, List<string> emailAddress)
        {
            var template = govUkNotifyConfig.ProceedApplicationConsentToDIPTemplate;

            var personalisation = new Dictionary<string, dynamic>
                {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.CompanyNumber,  companyNumber},
                { template.CompanyAddress,  companyAddress},
                { template.PublicContactEmail,  publicContactEmail},
                { template.PublicPhoneNumber ,  publicPhoneNumber},
                { template.ConsentLink ,  consentLink }


                };

            return await SendNotifications(emailAddress, template, personalisation);

        }
        #endregion

    }
}
