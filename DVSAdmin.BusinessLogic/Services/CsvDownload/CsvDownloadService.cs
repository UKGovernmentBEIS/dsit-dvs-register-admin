using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using DVSAdmin.Data;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services
{
    public sealed class PfrCsvMap : ClassMap<Service>
    {
        public PfrCsvMap()
        {
            //Maps Service table columns to Register fields
            Map(s => s.Provider.RegisteredName).Name("Provider");
            Map(s => s.ServiceName).Name("Service name");
            Map(s => s.ServiceStatus).Name("Status");
            Map(s => s.CabUser.Cab.CabName).Name("CAB");
            Map(s => s.PublishedTime).Name("Published on");
            Map(s => s.ConformityIssueDate).Name("Certificate Issue Date");
            Map(s => s.ConformityExpiryDate).Name("Certificate Expiry Date");
        }
    }
    public class CsvDownloadService : ICsvDownloadService
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<CsvDownloadService> logger;

        public CsvDownloadService(
            DVSAdminDbContext context,
            ILogger<CsvDownloadService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<CsvDownload> DownloadAsync()
        {
            try
            {
                var services = await context.Service
                    .AsNoTracking()//Read only, so no need for tracking query
                    .Include(service => service.Provider)
                    .Include(service => service.CabUser.Cab)
                    .Where(service => service.ServiceStatus == ServiceStatusEnum.Published)
                    // .Select(service => new //Only show an extract that resembles the PFR
                    // {
                    //     ProviderName = service.Provider.RegisteredName,
                    //     service.ServiceName,
                    //     service.ServiceStatus,
                    //     service.ServiceSupSchemeMapping,
                    //     service.CabUser.Cab.CabName,
                    //     service.PublishedTime,
                    //     service.ConformityIssueDate,
                    //     service.ConformityExpiryDate
                    // })
                    .ToListAsync();

                if (!services.Any())
                {
                    throw new InvalidOperationException("No service data available for export");
                }

                // Generate the CSV in memory
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    Quote = '"',
                    //Quote everything in the first row/header, and any value that has a comma
                    ShouldQuote = args => args.Row.Index == 0 || args.Field.Contains(",")
                });

                //Maps Service table columns to Register fields
                csv.Context.RegisterClassMap<PfrCsvMap>();

                await csv.WriteRecordsAsync(services);
                await writer.FlushAsync();

                logger.LogInformation("Successfully generated Register export with {Count} services", 
                    services.Count);

                // Return everything needed for the file download
                return new CsvDownload
                {
                    FileContent = memoryStream.ToArray(),
                    ContentType = "text/csv",
                    FileName = $"dvs-register_{DateTime.Now:ddMMyyyy}.csv"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not generate Register export");
                throw;
            }
        }
    }
}