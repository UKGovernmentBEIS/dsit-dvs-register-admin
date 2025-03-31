using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class RegManagementEmailSender: EmailSender
    {
        public RegManagementEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }

        #region Register Management
        public async Task<bool> SendServicePublishedToDIP(string recipientName, string serviceName, string companyName, string emailAddress)
        {
            var template = govUkNotifyConfig.ServicePublishedDIPTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.RecipientName,  recipientName},
                { template.CompanyName,  companyName}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendServicePublishedToCAB(string recipientName, string serviceName, string companyName, string emailAddress)
        {
            var template = govUkNotifyConfig.ServicePublishedCABTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.RecipientName,  recipientName},
                { template.CompanyName,  companyName}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendServicePublishedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.ServicePublishedDSITTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName}

             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        #endregion
    }
}
