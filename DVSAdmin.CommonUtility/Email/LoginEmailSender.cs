using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility.Email
{
    public class LoginEmailSender : EmailSender
    {

        public LoginEmailSender(GovUkNotifyApi govUkNotifyApi, IOptions<GovUkNotifyConfiguration> config) : base(govUkNotifyApi, config)
        {

        }
       
        public async Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.AccountCreatedTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {

                { template.RecipientName,  recipientName},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            return await SendNotification(emailAddress, template, personalisation);
        }

        public async Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress)
        {
            var template = govUkNotifyConfig.FailedLoginAttemptTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.TimeStamp,  timestamp}  ,
                { template.Email,  emailAddress}
            };

            return await SendNotificationToOfDiaCommonMailBox(template, personalisation);
        }    



    }
}
