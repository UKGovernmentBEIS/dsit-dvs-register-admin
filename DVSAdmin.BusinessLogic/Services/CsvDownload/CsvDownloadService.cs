using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using DVSAdmin.Data;
using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
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