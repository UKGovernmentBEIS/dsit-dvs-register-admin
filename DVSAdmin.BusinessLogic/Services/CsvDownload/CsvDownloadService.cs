using CsvHelper;
using CsvHelper.Configuration;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DVSAdmin.BusinessLogic.Services
{
    public sealed class PfrCsvMap : ClassMap<Service>
    {
        public PfrCsvMap()
        {
            //Maps Service table columns to Register fields
            Map(s => s.Provider.RegisteredName).Name("Provider");
            Map(s => s.ServiceName).Name("Service name");
            Map(s => s.ServiceStatusDescription) .Name("Status");
            Map(s => s.CabUser.Cab.CabName).Name("CAB");
            Map(s => s.PublishedTime).Name("Published on");
            Map(s => s.ConformityIssueDate).Name("Certificate Issue Date");
            Map(s => s.ConformityExpiryDate).Name("Certificate Expiry Date");
        }
    }
    public class CsvDownloadService : ICsvDownloadService
    {
        private readonly IRegManagementRepository regManagementRepository;
     
        private readonly ILogger<CsvDownloadService> logger;

        public CsvDownloadService(IRegManagementRepository regManagementRepository,ILogger<CsvDownloadService> logger)
        {
            this.regManagementRepository = regManagementRepository;
            this.logger = logger;
        }

        public async Task<CsvDownload> DownloadAsync()
        {
            try
            {
                var services = await regManagementRepository.GetPublishedServices();

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