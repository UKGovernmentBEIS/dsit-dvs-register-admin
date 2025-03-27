using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class EditEmailSender : EmailSender
    {
        public EditEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }
        public async Task<bool> ProviderEditRequest(string emailAddress, string recipientName, string companyName, string currentData, string previousData, string link)
        {
            var template = govUkNotifyConfig.EditProviderRequestTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.RecipientName,  recipientName},
                { template.PreviousData,  previousData},
                { template.CurrentData,  currentData},
                { template.ApproveLink,  link}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> ServiceEditRequest(string emailAddress, string recipientName, string companyName, string serviceName, string currentData, string previousData, string link)
        {
            var template = govUkNotifyConfig.EditServiceRequestTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.RecipientName,  recipientName},
                { template.PreviousData,  previousData},
                { template.CurrentData,  currentData},
                { template.ApproveLink,  link}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> ProviderEditRequestConfirmation(string loggedInUser, string recipientName, string companyName, string currentData, string previousData)
        {
            var template = govUkNotifyConfig.EditProviderRequestConfirmationTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.RecipientName,  recipientName},
                { template.PreviousData,  previousData},
                { template.CurrentData,  currentData}
             };

            return await SendNotification(loggedInUser, template, personalisation);
        }

        public async Task<bool> ServiceEditRequestConfirmation(string loggedInUser, string recipientName, string companyName, string serviceName, string currentData, string previousData)
        {
            var template = govUkNotifyConfig.EditServiceRequestConfirmationTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.RecipientName,  recipientName},
                { template.PreviousData,  previousData},
                { template.CurrentData,  currentData}
             };
            return await SendNotification(loggedInUser, template, personalisation);
        }
    }
}
