using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class PICheckEmailSender: EmailSender
    {
        public PICheckEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }

        #region PI Check
        public async Task<bool> SendPrimaryCheckPassConfirmationToDSIT(string companyName, string serviceName, string expirationDate)
        {
            var template = govUkNotifyConfig.PassPIPrimaryCheckTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ExpirationDate, expirationDate  },
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendPrimaryCheckFailConfirmationToDSIT(string companyName, string serviceName, string expirationDate)
        {
            var template = govUkNotifyConfig.FailPIPrimaryCheckTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ExpirationDate, expirationDate  },
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendPrimaryCheckRoundTwoConfirmationToDSIT(string companyName, string serviceName, string expirationDate)
        {
            var template = govUkNotifyConfig.PICheckRoundTwoTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                 { template.ExpirationDate, expirationDate  },
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendApplicationRejectedToDIP(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.PICheckApplicationRejectedDIPTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName}
            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendApplicationRejectedConfirmationToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.PICheckApplicationRejectedDISTTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName, companyName}
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendApplicationApprovedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.PICheckApplicationApprovedDISTTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName}
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        #endregion

        #region Closing the loop

        public async Task<bool> SendConsentToPublishToDIP(string companyName, string serviceName, string recipientName, string consentLink, string emailAddress)
        {
            var template = govUkNotifyConfig.DIPConsentToPublishTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.RecipientName,  recipientName},
                { template.ConsentLink,  consentLink}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        #endregion
    }
}
