using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class RemoveProviderService : IRemoveProviderService
    {
        private readonly IRemoveProviderRepository removeProviderRepository;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IMapper automapper;
        private readonly RemoveProviderEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;
        



        public RemoveProviderService(IRemoveProviderRepository removeProviderRepository, IMapper automapper,
          RemoveProviderEmailSender emailSender, ICertificateReviewService certificateReviewService, IJwtService jwtService, IConfiguration configuration)
        {
            this.removeProviderRepository = removeProviderRepository;
            this.certificateReviewService = certificateReviewService;
            this.automapper = automapper;
            this.emailSender = emailSender;      
            this.jwtService = jwtService;
            this.configuration = configuration;            
        }
        public async Task<ServiceDto> GetServiceDetails(int serviceId)
        {
            var service = await removeProviderRepository.GetServiceDetails(serviceId);
            ServiceDto serviceDto = automapper.Map<ServiceDto>(service);
            return serviceDto;
        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerProfileId)
        { 
            var provider = await removeProviderRepository.GetProviderDetails(providerProfileId);
            ProviderProfileDto providerDto = automapper.Map<ProviderProfileDto>(provider);
            providerDto.Services = ServiceHelper.FilterByServiceStatusAndLatestModifiedDate(providerDto.Services);
            return providerDto;
        }

        public async Task<GenericResponse> RemoveServiceRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, ServiceRemovalReasonEnum? serviceRemovalReason)
        {
            GenericResponse genericResponse = await removeProviderRepository.RemoveServiceRequest(providerProfileId, serviceIds, loggedInUserEmail, serviceRemovalReason);

            if (genericResponse.Success)
            {
                genericResponse = await GenerateTokenAndSendServiceRemoval(providerProfileId, serviceIds, loggedInUserEmail, serviceRemovalReason, false);                     
            }
            return genericResponse;
        }
        public async Task<GenericResponse> RemoveProviderRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, RemovalReasonsEnum? reason)
        {
            GenericResponse genericResponse = await removeProviderRepository.RemoveProviderRequest(providerProfileId, serviceIds, loggedInUserEmail, reason);
            TeamEnum requstedBy = reason == RemovalReasonsEnum.ProviderRequestedRemoval ? TeamEnum.Provider : TeamEnum.DSIT;
            if (genericResponse.Success)
            {                
                genericResponse = await GenerateTokenAndSendProviderRemoval(providerProfileId, serviceIds, loggedInUserEmail, dsitUserEmails, requstedBy, reason, false);                
            }            
            return genericResponse;
        }

        public async Task<GenericResponse> GenerateTokenAndSendServiceRemoval(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, ServiceRemovalReasonEnum? serviceRemovalReason, bool isResend)
        {

            ProviderProfile providerProfile = await removeProviderRepository.GetProviderAndServices(providerProfileId);
            await UpdateProviderStatusByStatusPriority(providerProfile, loggedInUserEmail, EventTypeEnum.RemoveService);
            // save token for 2i check
            //Insert token details to db for further reference, if multiple services are removed, insert to mapping table

            TokenDetails tokenDetails = jwtService.GenerateToken(string.Empty, providerProfile.Id, string.Join(",", serviceIds));

            ICollection<RemoveTokenServiceMapping> removeTokenServiceMapping = [];
            foreach (var item in serviceIds)
            {
                removeTokenServiceMapping.Add(new RemoveTokenServiceMapping { ServiceId = item });
            }

            RemoveProviderToken removeProviderToken = new()
            {
                ProviderProfileId = providerProfile.Id,
                Token = tokenDetails.Token,
                TokenId = tokenDetails.TokenId,
                RemoveTokenServiceMapping = removeTokenServiceMapping
            };
            GenericResponse genericResponse = await removeProviderRepository.SaveRemoveProviderToken(removeProviderToken, TeamEnum.DSIT, EventTypeEnum.RemoveService, loggedInUserEmail, isResend);

            if (genericResponse.Success)
            {
                var filteredServiceNames = providerProfile.Services.Where(s => serviceIds.Contains(s.Id)).Select(s => s.ServiceName).ToList();
                string serviceNames = string.Join("\r", filteredServiceNames);
                string reasonString = ServiceRemovalReasonEnumExtensions.GetDescription(serviceRemovalReason.Value);

                string linkForEmailToProvider = configuration["DvsRegisterLink"] + "remove-provider/provider/provider-details?token=" + tokenDetails.Token;
                //37/Provider/Service removal request
                await emailSender.SendRequestToRemoveServiceToProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, serviceNames, reasonString, linkForEmailToProvider);
                await emailSender.SendRequestToRemoveServiceToProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, serviceNames, reasonString, linkForEmailToProvider);
                await emailSender.RequestToRemoveServiceNotificationToDSIT(serviceNames, providerProfile.RegisteredName, reasonString);//38/DSIT/service removal request sent
            }
            return genericResponse;
        }
        public async Task<GenericResponse> GenerateTokenAndSendProviderRemoval(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, TeamEnum requstedBy,  RemovalReasonsEnum? reason, bool isResend)
        {
            // save token for 2i check
            //Insert token details to db for further reference, if multiple services are removed, insert to mapping table
            TokenDetails tokenDetails = jwtService.GenerateToken(requstedBy == TeamEnum.DSIT ? "DSIT" : string.Empty, providerProfileId, string.Join(",", serviceIds));
            RemoveProviderToken removeProviderToken = new()
            {                
                ProviderProfileId = providerProfileId,
                Token = tokenDetails.Token,
                TokenId = tokenDetails.TokenId
            };
            GenericResponse genericResponse = await removeProviderRepository.SaveRemoveProviderToken(removeProviderToken, TeamEnum.DSIT, EventTypeEnum.RemoveProvider, loggedInUserEmail, isResend);
            if (genericResponse.Success)
            {
                ProviderProfile providerProfile = await removeProviderRepository.GetProviderAndServices(providerProfileId);
                string reasonString = RemovalReasonsEnumExtensions.GetDescription(reason.Value);
                await SendEmails(serviceIds, dsitUserEmails, reasonString, requstedBy, tokenDetails, providerProfile, loggedInUserEmail);
            }
            return genericResponse;
        }

        public async Task<GenericResponse> UpdateProviderStatusByStatusPriority(int providerProfileId, string loggedInUserEmail, EventTypeEnum eventType, TeamEnum team = TeamEnum.DSIT)
        {
            ProviderProfile providerProfile = await removeProviderRepository.GetProviderAndServices(providerProfileId);
            ProviderStatusEnum providerStatus = ServiceHelper.GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
            return await removeProviderRepository.UpdateProviderStatus(providerProfileId, providerStatus, loggedInUserEmail, eventType, team);
        }

        public async Task<GenericResponse> UpdateProviderStatusByStatusPriority(ProviderProfile providerProfile, string loggedInUserEmail, EventTypeEnum eventType, TeamEnum team = TeamEnum.DSIT)
        {
            ProviderStatusEnum providerStatus = ServiceHelper.GetProviderStatus(providerProfile.Services, providerProfile.ProviderStatus);
            return await removeProviderRepository.UpdateProviderStatus(providerProfile.Id, providerStatus, loggedInUserEmail, eventType, team);
        }


        private async Task SendEmails(List<int> serviceIds, List<string> dsitUserEmails, string reason, TeamEnum requstedBy, TokenDetails tokenDetails, ProviderProfile providerProfile, string loggedInUserEmail)
        {
            var filteredServiceNames = providerProfile.Services.Where(s => serviceIds.Contains(s.Id)).Select(s => s.ServiceName).ToList();
            string serviceNames = string.Join("\r", filteredServiceNames);

            if (requstedBy == TeamEnum.Provider)
            {
                string linkForEmailToProvider = configuration["DvsRegisterLink"] + "remove-provider/provider/provider-details?token=" + tokenDetails.Token;
                await emailSender.SendRequestToRemoveRecordToProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, linkForEmailToProvider);//29/Provider/Request to remove record
                await emailSender.SendRequestToRemoveRecordToProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, linkForEmailToProvider);//30/DSIT/Record removal request sent
                await emailSender.SendRecordRemovalRequestConfirmationToDSIT(providerProfile.RegisteredName, serviceNames);
            }
            else if (requstedBy == TeamEnum.DSIT)
            {
                await emailSender.RemovalRequestForApprovalToDSIT(loggedInUserEmail, serviceNames, providerProfile.RegisteredName, reason);
                string linkForEmailToDSIT = configuration["DvsRegisterLink"] + "remove-provider/dsit/provider-details?token=" + tokenDetails.Token;
                foreach (var email in dsitUserEmails)
                {
                    await emailSender.SendRemoval2iCheckToDSIT(email, email, linkForEmailToDSIT, providerProfile.RegisteredName, serviceNames, reason);
                }

            }
        }

        public async Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string>? dsitUserEmails)
        {
            GenericResponse genericResponse = await removeProviderRepository.RemoveServiceRequestByCab(providerProfileId, serviceIds, loggedInUserEmail);
                       
            if (genericResponse.Success)
            {        
                 List<string>  cabEmails = await certificateReviewService.GetCabEmailListForProvider(providerProfileId);
                // update provider status
                
                // save token for 2i check
                ProviderProfile providerProfile = await removeProviderRepository.GetProviderDetails(providerProfileId);
                
                genericResponse = await UpdateProviderStatusByStatusPriority(providerProfile, loggedInUserEmail, EventTypeEnum.RemoveServiceRequestedByCab);

                Service service = providerProfile.Services.Where(s => s.Id == serviceIds[0]).FirstOrDefault(); // only single service removal in current release
               if (genericResponse.Success && providerProfile.ProviderStatus == ProviderStatusEnum.RemovedFromRegister)
                {
                    //35/CAB + Provider/Whole record
                    foreach (var cabEmail in cabEmails)
                    {
                        await emailSender.RecordRemovedConfirmedToCabOrProvider(cabEmail, cabEmail, providerProfile.RegisteredName, service.ServiceName, Constants.CabHasWithdrawnCertificate);
                    }
                    await emailSender.SendRecordRemovedToDSIT(providerProfile.RegisteredName, service.ServiceName, Constants.CabHasWithdrawnCertificate);
                    await emailSender.RecordRemovedConfirmedToCabOrProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, providerProfile.RegisteredName, service.ServiceName, Constants.CabHasWithdrawnCertificate);
                    await emailSender.RecordRemovedConfirmedToCabOrProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, providerProfile.RegisteredName, service.ServiceName, Constants.CabHasWithdrawnCertificate);
                }
                else
                {
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.CabUser.CabEmail, service.CabUser.CabEmail, service.ServiceName, Constants.CabHasWithdrawnCertificate);//41/CAB + Provider/Service removed
                    await emailSender.ServiceRemovedToDSIT(service.ServiceName, Constants.CabHasWithdrawnCertificate); //42/DSIT/Service removed
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.Provider.PrimaryContactFullName, service.Provider.PrimaryContactEmail, service.ServiceName, Constants.CabHasWithdrawnCertificate);//41/CAB + Provider/Service removed
                    await emailSender.ServiceRemovedConfirmedToCabOrProvider(service.Provider.SecondaryContactFullName, service.Provider.SecondaryContactEmail, service.ServiceName, Constants.CabHasWithdrawnCertificate);//41/CAB + Provider/Service removed
                }
             
            }

            return genericResponse;
        }

        public async Task<GenericResponse> CancelRemoveServiceRequest(int providerProfileId, int serviceId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = await removeProviderRepository.CancelRemoveServiceRequest(providerProfileId, serviceId, loggedInUserEmail);

            if (genericResponse.Success)
            {
                ProviderProfile providerProfile = await removeProviderRepository.GetProviderDetails(providerProfileId);
                genericResponse = await UpdateProviderStatusByStatusPriority(providerProfile, loggedInUserEmail, EventTypeEnum.RemoveServiceRequestedByCab);

                if (genericResponse.Success)
                {
                    var service = providerProfile.Services.FirstOrDefault(s => s.Id == serviceId);
                    await emailSender.CancelServiceRemovalRequestToProvider(providerProfile.PrimaryContactFullName, providerProfile.PrimaryContactEmail, service.ServiceName);
                    await emailSender.CancelServiceRemovalRequestToProvider(providerProfile.SecondaryContactFullName, providerProfile.SecondaryContactEmail, service.ServiceName);                    
                    await emailSender.CancelServiceRemovalRequestToDSIT(providerProfile.RegisteredName, service.ServiceName);
                }
            }

            return genericResponse;
        }
    }
}