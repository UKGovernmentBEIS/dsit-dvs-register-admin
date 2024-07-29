using DVSAdmin.CommonUtility.Models;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DVSAdmin.CommonUtility
{
    public class BucketService : IBucketService
    {
        private readonly S3Configuration config;
        private readonly ILogger logger;
        private readonly AmazonS3Client s3Client;


        public BucketService(IOptions<S3Configuration> options, ILogger<BucketService> logger, AmazonS3Client s3Client)
        {
            config = options.Value;
            this.logger = logger;
            this.s3Client = s3Client;
        }

       
        /// <summary>
        /// Download file from s3
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public async Task<byte[]?> DownloadFileAsync(string keyName)
        {
            try
            {

                var request = new GetObjectRequest
                {
                    BucketName = config.BucketName,
                    Key = keyName
                };

                using (var response = await s3Client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (AmazonS3Exception e)
            {
                logger.LogError("AWS S3 error when reading  file from bucket: '{0}', key: '{1}'. Message:'{2}'", config.BucketName, keyName, e.Message);
                return null;
            }
            catch (Exception e)
            {
                logger.LogError("Error when reading file from bucket: '{0}', key: '{1}'. Message:'{2}'", config.BucketName, keyName, e.Message);
                return null;
            }
        }
    }
}

