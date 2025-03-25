using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class RemoveProviderEmailSender : EmailSender
    {

        public RemoveProviderEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }


        #region Remove emails
        public async Task<bool> SendRequestToRemoveRecordToProvider(string recipientName, string emailAddress, string confirmationLink)
        {
            var template = govUkNotifyConfig.RequestToRemoveRecordToProvider;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ConfirmationLink,  confirmationLink}

             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendRemoval2iCheckToDSIT(string recipientName, string emailAddress, string removalLink, string companyName, string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.Removal2iCheckToDSITTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.RemovalLink,  removalLink},
                { template.ReasonForRemoval,reasonForRemoval}

             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendRecordRemovalRequestConfirmationToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.RecordRemovalRequestSentConfirmationToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},


             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendRequestToRemoveServiceToProvider(string recipientName, string emailAddress, string serviceName, string reason, string removalLink)
        {
            var template = govUkNotifyConfig.RequestToRemoveServiceToProvider;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reason},
                { template.RemovalLink,  removalLink}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> RemovalRequestForApprovalToDSIT(string emailAddress, string serviceName, string companyName, string reason)
        {

            var template = govUkNotifyConfig.RemovalRequestForApprovalToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
               { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reason}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> RequestToRemoveServiceNotificationToDSIT(string serviceName, string companyName, string reason)
        {

            var template = govUkNotifyConfig.RequestToRemoveServiceNotificationToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
               { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reason}
             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> ServiceRemovedConfirmedToCabOrProvider(string recipientName, string emailAddress, string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.ServiceRemovedConfirmedToCabOrProvider;
            var personalisation = new Dictionary<string, dynamic>
            {
                 { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval},
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> ServiceRemovedToDSIT(string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.ServiceRemovedToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {

                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval},
             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> RecordRemovedConfirmedToCabOrProvider(string recipientName, string emailAddress, string companyName, string serviceName, string reasonForRemoval)
        {

            var template = govUkNotifyConfig.RecordRemovedConfirmedToCabOrProvider;
            var personalisation = new Dictionary<string, dynamic>
            {
                 { template.RecipientName,  recipientName},
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval},
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendRecordRemovedToDSIT(string companyName, string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.RecordRemovedToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval},
             };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendServiceRemoval2iCheckToDSIT(string emailAddress, string removalLink, string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.RemoveService2iCheckToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval},
                { template.RemovalLink,  removalLink}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> ServiceRemovalRequestCreated(string emailAddress, string serviceName, string reasonForRemoval)
        {
            var template = govUkNotifyConfig.ServiceRemovalRequestCreated;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.ReasonForRemoval,  reasonForRemoval}
             };
            return await SendNotification(emailAddress, template, personalisation);
        }


        #endregion
    }
}
