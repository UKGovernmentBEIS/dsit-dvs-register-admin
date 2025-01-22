using DVSAdmin;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Data;
using DVSAdmin.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration, builder.Environment);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

Console.WriteLine(environment);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile($"appsettings.{environment}.json", optional: true)
.AddEnvironmentVariables();


startup.ConfigureServices(builder.Services);
var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DVSAdminDbContext>();
startup.ConfigureDatabaseHealthCheck(dbContext);

// Configure Forwarded Headers Middleware
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Clear the default settings for KnownNetworks and KnownProxies
forwardedHeadersOptions.KnownNetworks.Clear(); // Clear default networks
forwardedHeadersOptions.KnownProxies.Clear();  // Clear default proxies
app.UseForwardedHeaders(forwardedHeadersOptions);


//// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseHangfireDashboard();
    app.UseMiddleware<BasicAuthMiddleware>();
}
else
{
    app.UseMiddleware<ExceptionHandlerMiddleware>();
}

var jobRunner = app.Services.GetService<IRecurringJobManager>();

// Check for expired certificates at 17:00 daily            
jobRunner.AddOrUpdate<BackgroundJobService>(
    "Set status of expired certificates to Removed",
    service => service.RemoveExpiredCertificates(),
    "* * * * *"); // every minute

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapControllers();

app.Run();

