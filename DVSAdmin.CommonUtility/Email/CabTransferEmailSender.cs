using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class CabTransferEmailSender : EmailSender
    {
        public CabTransferEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {
        }
        public async Task<bool> SendCabTransferConfirmationToDSTI(string acceptingCabName, string providerName, string serviceName)
        {
            var template = govUkNotifyConfig.CabTransferConfirmationToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.AcceptingCabName, acceptingCabName },
                { template.ProviderName, providerName },
                { template.ServiceName, serviceName }
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendCabTransferConfirmationToCAB(string userName, string providerName, string serviceName, string emailAddress)
        {
            var template = govUkNotifyConfig.CabTransferConfirmationToCAB;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CabUserName, userName },
                { template.ProviderName, providerName },
                { template.ServiceName, serviceName }
            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendCabTransferCancellationToDSTI(string providerName, string serviceName)
        {
            var template = govUkNotifyConfig.CabTransferCancelledToDSIT;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ProviderName, providerName },
                { template.ServiceName, serviceName }
            };
            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }

        public async Task<bool> SendCabTransferCancellationToCAB(string userName, string providerName, string serviceName, string emailAddress)
        {
            var template = govUkNotifyConfig.CabTransferCancelledToCAB;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CabUserName, userName },
                { template.ProviderName, providerName },
                { template.ServiceName, serviceName }
            };
            return await SendNotification(emailAddress, template, personalisation);
        }
    }
}
