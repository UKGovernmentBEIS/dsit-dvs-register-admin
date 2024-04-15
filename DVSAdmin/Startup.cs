using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data;
using DVSAdmin.Middleware;
using DVSAdmin.BusinessLogic;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Data.Repositories;

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
            ConfigureDvsRegisterServices(services);
            ConfigureAutomapperServices(services);

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
            services.AddScoped<IPreRegistrationReviewService, PreRegistrationReviewService>();
        }
        public void ConfigureAutomapperServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
        }
    }
}
