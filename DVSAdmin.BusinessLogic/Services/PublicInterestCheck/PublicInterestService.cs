using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.Data.Repositories;
using DVSAdmin.Data.Repositories.PublicInterestCheck;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services.PublicInterestCheck
{
    public class PublicInterestService : IPublicInterestCheckService
    {

        private readonly IPublicInterestCheckRepository publicInterestCheckRepository;
        private readonly IConsentRepository consentRepository;
        private readonly IMapper automapper;
        private readonly IEmailSender emailSender;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;


        public PublicInterestService(IPublicInterestCheckRepository publicInterestCheckRepository, IMapper automapper,
          IEmailSender emailSender, IConsentRepository consentRepository, IJwtService jwtService, IConfiguration configuration)
        {
            this.publicInterestCheckRepository = publicInterestCheckRepository;
            this.automapper = automapper;
            this.emailSender = emailSender;
            this.consentRepository = consentRepository;
            this.jwtService = jwtService;
            this.configuration = configuration;
        }
        public async Task<List<Service>> GetPICheckList()
        {
            var publicinterestchecks = await publicInterestCheckRepository.GetPICheckList();
            return automapper.Map<List<Service>>(publicinterestchecks);
        }
    }
}