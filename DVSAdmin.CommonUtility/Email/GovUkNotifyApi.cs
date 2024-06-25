using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using System.Net.Mail;

namespace DVSAdmin.CommonUtility.Email
{
    public class GovUkNotifyApi : IEmailSender
    {
        private readonly NotificationClient client;
        private readonly GovUkNotifyConfiguration govUkNotifyConfig;
        private readonly ILogger<GovUkNotifyApi> logger;     

        public GovUkNotifyApi(IOptions<GovUkNotifyConfiguration> config, ILogger<GovUkNotifyApi> logger)
        {
            govUkNotifyConfig = config.Value;
            client = new NotificationClient(govUkNotifyConfig.ApiKey);
            this.logger = logger;           
        }


        private async Task<bool> SendEmail(GovUkNotifyEmailModel emailModel)
        {
            try
            {
                await client.SendEmailAsync(
                    emailModel.EmailAddress,
                    emailModel.TemplateId,
                    emailModel.Personalisation,
                    emailModel.Reference,
                    emailModel.EmailReplyToId);
                    return true;
            }
            catch (NotifyClientException e)
            {
                if (e.Message.Contains("Not a valid email address"))
                {
                    logger.LogWarning("GOV.UK Notify could not send to an invalid email address");
                }
                else if (e.Message.Contains("send to this recipient using a team-only API key"))
                {
                    // In development we use a 'team-only' API key which can only send to team emails
                    logger.LogWarning("GOV.UK Notify cannot send to this recipient using a team-only API key");
                }
                else
                {
                    logger.LogError(e, "GOV.UK Notify returned an error");
                }
                return false;
            }
        }
      

        public async Task<bool> SendPrimaryCheckPassConfirmationToOfDia(string URN, string expirationDate)
        {
            var template = govUkNotifyConfig.PassPrimaryCheckTemplate;          

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ExpirationDate, expirationDate  },
                { template.URN,  URN},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendPrimaryCheckFailConfirmationToOfDia(string URN, string expirationDate)
        {
            var template = govUkNotifyConfig.FailPrimaryCheckTemplate;          

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ExpirationDate, expirationDate  },
                { template.URN,  URN},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendPrimaryCheckRoundTwoConfirmationToOfDia(string URN, string expirationDate)
        {
            var template = govUkNotifyConfig.PrimaryCheckRoundTwoTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ExpirationDate, expirationDate  },
                { template.URN,  URN},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendPrimaryApplicationRejectedConfirmationToOfDia(string URN)
        {
            var template = govUkNotifyConfig.ApplicationRejectedTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                              
                { template.URN,  URN},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendURNIssuedConfirmationToOfDia(string URN)
        {
            var template = govUkNotifyConfig.IssueURNConfirmationTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {

                { template.URN,  URN},
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendAccountCreatedConfirmation(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.AccountCreatedTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {

                { template.RecipientName,  recipientName},               
                { template.LoginLink, govUkNotifyConfig.LoginLink }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendFailedLoginAttempt(string timestamp, string emailAddress)
        {
            var template = govUkNotifyConfig.FailedLoginAttemptTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.TimeStamp,  timestamp}               
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

       
        public async Task<bool> SendApplicationRejectedToDIASP(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.ApplicationRejectedConfirmationTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName}               
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendApplicationApprovedToDIASP(string recipientName, string URN, string expiryDate, string emailAddress)
        {
            var template = govUkNotifyConfig.ApplicationApprovedConfirmationTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ExpiryDate,  expiryDate},
                { template.URN,  URN},
                { template.PreRegLink,  govUkNotifyConfig.PreRegLink},
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendCertificateInfoApprovedToCab(string recipientName, string URN, string serviceName, string emailAddress)
        {
            var template = govUkNotifyConfig.CertificateInfoApprovedToCabTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.URN,  URN}
            
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendCertificateInfoApprovedToDSIT(string URN, string serviceName)
        {
            var template = govUkNotifyConfig.CertificateInfoApprovedToDSIT;

            var personalisation = new Dictionary<string, dynamic>
            {               
                { template.ServiceName,  serviceName},
                { template.URN,  URN}

            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendCertificateInfoRejectedToCab(string recipientName, string URN, string serviceName, string emailAddress)
        {
            var template = govUkNotifyConfig.CertificateInfoRejectedToCabTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.RecipientName,  recipientName},
                { template.ServiceName,  serviceName},
                { template.URN,  URN}

            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendCertificateInfoRejectedToDSIT(string URN, string serviceName, string rejectionCategory, string rejectionComments)
        {
            var template = govUkNotifyConfig.CertificateInfoRejectedToDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {             
                { template.ServiceName,  serviceName},
                { template.URN,  URN},
                { template.RejectionCategory,  rejectionCategory},
                { template.RejectionComments,  rejectionComments},


             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendConsentToPublishToDIP(string URN, string serviceName, string recipientName, string emailAddress, string consentLink)
        {
            var template = govUkNotifyConfig.DIPConsentToPublishTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.URN,  URN},
                { template.RecipientName,  recipientName},
                { template.ConsentLink,  consentLink}
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendConsentToPublishToAdditionalContact(string URN, string serviceName, string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.AdditionalContactConsentTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.URN,  URN},
                { template.RecipientName,  recipientName}
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendAgreementToPublishToDIP(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.AgreementToPublishTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
               
                { template.RecipientName,  recipientName}
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendAgreementToPublishToDSIT(string URN, string serviceName)
        {
            var template = govUkNotifyConfig.AgreementToPublishToDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.URN,  URN},
                { template.ServiceName,  serviceName},
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }
    }
}
