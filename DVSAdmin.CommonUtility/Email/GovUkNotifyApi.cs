using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;

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
                if(emailModel.EmailList != null && emailModel.EmailList.Any()) 
                { 
                    foreach(var email in emailModel.EmailList)
                    {
                    await client.SendEmailAsync(
                    email,
                    emailModel.TemplateId,
                    emailModel.Personalisation,
                    emailModel.Reference,
                    emailModel.EmailReplyToId);
                              
                    }
                    return true;
                }
                else
                {
                   await client.SendEmailAsync(
                   emailModel.EmailAddress,
                   emailModel.TemplateId,
                   emailModel.Personalisation,
                   emailModel.Reference,
                   emailModel.EmailReplyToId);
                   return true;

                }
               
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


        #region Common
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
                { template.TimeStamp,  timestamp}  ,
                { template.Email,  emailAddress}
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }
        #endregion

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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendApplicationRejectedToDIP(string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.PICheckApplicationRejectedDIPTemplate;

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

        public async Task<bool> SendApplicationRejectedConfirmationToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.PICheckApplicationRejectedDISTTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName, companyName}
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendApplicationApprovedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.PICheckApplicationApprovedDISTTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName}
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        #endregion

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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendCertificateInfoApprovedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.CertificateInfoApprovedToDSIT;

            var personalisation = new Dictionary<string, dynamic>
            {               
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName}

            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }



        #endregion


        #region Opening the loop
        public async Task<bool> SendProceedApplicationConsentToDIP(string companyName, string serviceName, string companyNumber, string companyAddress, string publicContactEmail,
          string publicPhoneNumber, string consentLink, List<string> emailAddress)
        {
            var template = govUkNotifyConfig.ProceedApplicationConsentToDIPTemplate;

            try
            {
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
                var emailModel = new GovUkNotifyEmailModel
                {
                    EmailList =  emailAddress,
                    TemplateId = template.Id,
                    Personalisation = personalisation
                };
                return await SendEmail(emailModel);
            }
            catch (Exception ex)
            {

                throw;
            }


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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }
        public async Task<bool> SendAgreementToPublishToDIP(string companyName, string serviceName, string recipientName, string emailAddress)
        {
            var template = govUkNotifyConfig.AgreementToPublishTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {

                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName},
                { template.RecipientName,  recipientName},
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendAgreementToPublishToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.AgreementToPublishToDSITTemplate;

            var personalisation = new Dictionary<string, dynamic>
            {
                { template.CompanyName,  companyName},
                { template.ServiceName,  serviceName}
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }
        #endregion


        #region Register Management
        public async Task<bool> SendServicePublishedToDIP(string recipientName, string serviceName,string companyName, string emailAddress)
        {
            var template = govUkNotifyConfig.ServicePublishedDIPTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.RecipientName,  recipientName},
                { template.CompanyName,  companyName}
             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
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
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        public async Task<bool> SendServicePublishedToDSIT(string companyName, string serviceName)
        {
            var template = govUkNotifyConfig.ServicePublishedDSITTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.ServiceName,  serviceName},
                { template.CompanyName,  companyName}

             };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress =  govUkNotifyConfig.OfDiaEmailId,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            return await SendEmail(emailModel);
        }

        #endregion
    }
}
