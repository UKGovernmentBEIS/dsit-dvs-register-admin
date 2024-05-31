using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data;
using DVSAdmin.Middleware;
using DVSAdmin.BusinessLogic;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Data.Repositories;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;

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
            services.AddControllersWithViews();
            if(webHostEnvironment.IsDevelopment() || webHostEnvironment.IsStaging())
            {
                services.Configure<BasicAuthMiddlewareConfiguration>(
                    configuration.GetSection(BasicAuthMiddlewareConfiguration.ConfigSection));
            }
            string connectionString = string.Format(configuration.GetValue<string>("DB_CONNECTIONSTRING"));
            services.AddDbContext<DVSAdminDbContext>(opt =>
                opt.UseNpgsql(connectionString));
            ConfigureSession(services);
            ConfigureDvsRegisterServices(services);
            ConfigureAutomapperServices(services);
            ConfigureGovUkNotify(services);

        }

        private void ConfigureSession(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // ToDo:Adjust the timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
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
            services.AddScoped<IPreRegistrationReviewRepository, PreRegistrationReviewRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPreRegistrationReviewService, PreRegistrationReviewService>();
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
        }
        public void ConfigureAutomapperServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
        }
        private void ConfigureGovUkNotify(IServiceCollection services)
        {
            services.AddScoped<IEmailSender, GovUkNotifyApi>();
            services.Configure<GovUkNotifyConfiguration>(
                configuration.GetSection(GovUkNotifyConfiguration.ConfigSection));
        }
    }
}
