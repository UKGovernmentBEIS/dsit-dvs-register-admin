using Amazon;
using Amazon.S3;
using DVSAdmin.BusinessLogic;
using DVSAdmin.BusinessLogic.Models.Cookies;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.BusinessLogic.Services.CabTransfer;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Cookies;
using DVSAdmin.Data;
using DVSAdmin.Data.Repositories;
using DVSAdmin.Data.Repositories.BackgroundJobs;
using DVSAdmin.Data.Repositories.RegisterManagement;
using DVSAdmin.Data.Repositories.RemoveProvider;
using DVSAdmin.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace DVSAdmin
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = string.Format(configuration.GetValue<string>("DB_CONNECTIONSTRING"));
            // Add Hangfire services.
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString));

            // Add the Hangfire processing server as IHostedService
            services.AddHangfireServer();
            
            services.AddControllersWithViews();
            //Adding strict transport security header
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(1);
            });
            services.Configure<BasicAuthMiddlewareConfiguration>(
            configuration.GetSection(BasicAuthMiddlewareConfiguration.ConfigSection));
            
            // This allows encrypted cookies to be understood across multiple web server instances
            services.AddDataProtection().PersistKeysToDbContext<DVSAdminDbContext>();
            
            ConfigureDatabaseContext(services);
            ConfigureSession(services);
            ConfigureGovUkNotify(services);
            ConfigureDvsRegisterServices(services);
            ConfigureAutomapperServices(services);           
            ConfigureJwtServices(services);
            ConfigureCookieService(services);
            ConfigureS3Client(services);
            ConfigureS3FileReader(services);

        }

        private void ConfigureDatabaseContext(IServiceCollection services)
        {
            string connectionString = string.Format(configuration.GetValue<string>("DB_CONNECTIONSTRING"));
            services.AddDbContext<DVSAdminDbContext>(opt =>
                opt.UseNpgsql(connectionString));
        }

        private void ConfigureSession(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(360); 
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;               
            });
        }

        public void ConfigureDatabaseHealthCheck(DVSAdminDbContext? dbContext)
        {
            try
            {
                if (dbContext == null) throw new InvalidOperationException(Constants.DbContextNull);
                DbConnection conn = dbContext.Database.GetDbConnection();
                conn.Open();   // Check the database connection
                Console.WriteLine(Constants.DbConnectionSuccess);
                conn.Close();   // close the database connection               
            }
            catch (Exception ex)
            {
                Console.WriteLine(Constants.DbConnectionFailed + ex.Message);
                throw;
            }
        }

        public void ConfigureDvsRegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();           
            services.AddScoped<ISignUpService, SignUpService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped(opt =>
            {
                string userPoolId = string.Format(configuration.GetValue<string>("UserPoolId"));
                string clientId = string.Format(configuration.GetValue<string>("ClientId")); ;
                string region = string.Format(configuration.GetValue<string>("Region"));
                return new CognitoClient(userPoolId, clientId, region);
            });

            services.AddScoped<ICertificateReviewRepository, CertificateReviewRepository>();
            services.AddScoped<ICertificateReviewService, CertificateReviewService>();      
            services.AddScoped<IConsentRepository, ConsentRepository>();
            services.AddScoped<IRegManagementService, RegManagementService>();
            services.AddScoped<IRegManagementRepository, RegManagementRepository>();
            services.AddScoped<IPublicInterestCheckService, PublicInterestService>();
            services.AddScoped<IPublicInterestCheckRepository, PublicInterestCheckRepository>();
            services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
            services.AddScoped<ICsvDownloadService, CsvDownloadService>();
            services.AddScoped<IRemoveProviderService, RemoveProviderService>();
            services.AddScoped<IRemoveProviderRepository, RemoveProviderRepository>();
            services.AddScoped<IEditService, EditService>();
            services.AddScoped<IEditRepository, EditRepository>();
            services.AddScoped<ICabTransferService, CabTransferService>();
            services.AddScoped<ICabTransferRepository, CabTransferRepository>();
            services.AddTransient<LoginEmailSender>();
            services.AddTransient<CertificateReviewEmailSender>();
            services.AddTransient<PICheckEmailSender>();           
            services.AddTransient<RemoveProviderEmailSender>();
            services.AddTransient<EditEmailSender>();
            services.AddTransient<CabTransferEmailSender>();
        }
        public void ConfigureAutomapperServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
        }
        private void ConfigureGovUkNotify(IServiceCollection services)
        {
            services.AddSingleton<GovUkNotifyApi>();
            services.Configure<GovUkNotifyConfiguration>(
                configuration.GetSection(GovUkNotifyConfiguration.ConfigSection));
        }
        private void ConfigureJwtServices(IServiceCollection services)
        {
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.ConfigSection));
        }

        private void ConfigureS3Client(IServiceCollection services)
        {
            var s3Config = new S3Configuration();
            configuration.GetSection(S3Configuration.ConfigSection).Bind(s3Config);
            string localAccessKey = configuration.GetValue<string>("S3:LocalDevOnly_AccessKey")??string.Empty;
            string localSecreKey = configuration.GetValue<string>("S3:LocalDevOnly_SecretKey")??string.Empty;
            string localServiceURL = configuration.GetValue<string>("S3:LocalDevOnly_ServiceUrl")??string.Empty;


            if (!string.IsNullOrEmpty(localAccessKey) && !string.IsNullOrEmpty(localSecreKey) && !string.IsNullOrEmpty(localSecreKey))
            {

                // For local development connect to a local instance of Minio add the access key , secret key and service url in local user secrets only
                var clientConfig = new AmazonS3Config
                {
                    ServiceURL = localServiceURL,
                    ForcePathStyle = true,
                };

                services.AddScoped(_ => new AmazonS3Client(localAccessKey, localSecreKey, clientConfig));
            }

            else
            {
                services.AddScoped(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(s3Config.Region)));
            }

        }

        private void ConfigureS3FileReader(IServiceCollection services)
        {
            services.Configure<S3Configuration>(
                configuration.GetSection(S3Configuration.ConfigSection));
            services.AddScoped<IBucketService, BucketService>();
        }

        private void ConfigureCookieService(IServiceCollection services)
        {
            services.Configure<CookieServiceConfiguration>(
                configuration.GetSection(CookieServiceConfiguration.ConfigSection));

            // Change the default antiforgery cookie name so it doesn't include Asp.Net for security reasons
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "Antiforgery";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            services.AddSingleton<CookieService>();
        }
    }
}
