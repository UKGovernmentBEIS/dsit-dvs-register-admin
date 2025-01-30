using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICsvDownloadService
    {
        public Task<CsvDownload> DownloadAsync();
    }
}