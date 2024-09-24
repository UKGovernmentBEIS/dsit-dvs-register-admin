namespace DVSAdmin.CommonUtility
{
    public interface IBucketService
    {
        public Task<byte[]?> DownloadFileAsync(string keyName);

    }
}

