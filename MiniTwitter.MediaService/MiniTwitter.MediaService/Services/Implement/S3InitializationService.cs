using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MiniTwitter.MediaService.Models;

namespace MiniTwitter.MediaService.Services.Implement
{
    public class S3InitializationService
    {
        private readonly S3Configuration _config;

        public S3InitializationService(IOptions<S3Configuration> configOptions)
        {
            _config = configOptions.Value;
        }

        public async Task InitializeAsync()
        {
            int maxRetries = 5;
            int delay = 3000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);

                    var s3Config = new AmazonS3Config
                    {
                        ServiceURL = _config.ServiceURL,
                        ForcePathStyle = true,
                        RegionEndpoint = RegionEndpoint.GetBySystemName(_config.Region)
                    };

                    using var s3Client = new AmazonS3Client(credentials, s3Config);

                    Console.WriteLine($"🔍 تلاش {attempt}/{maxRetries}: بررسی اتصال به MinIO...");

                    // بررسی اتصال
                    var listResponse = await s3Client.ListBucketsAsync();
                    Console.WriteLine($"✅ اتصال به MinIO برقرار است");

                    // بررسی آیا Bucket موجود است
                    var bucketExists = listResponse.Buckets.Any(b => b.BucketName == _config.BucketName);

                    if (!bucketExists)
                    {
                        Console.WriteLine($"🔨 Bucket '{_config.BucketName}' موجود نیست. درحال ایجاد...");
                        
                        var createRequest = new PutBucketRequest
                        {
                            BucketName = _config.BucketName
                        };

                        await s3Client.PutBucketAsync(createRequest);
                        Console.WriteLine($"✅ Bucket '{_config.BucketName}' با موفقیت ایجاد شد");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Bucket '{_config.BucketName}' قبلاً موجود است");
                    }

                    return; // موفقیت‌آمیز
                }
                catch (AmazonS3Exception ex)
                {
                    Console.WriteLine($"⚠️ خطای S3 در تلاش {attempt}: {ex.Message}");
                    
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"⏳ صبر {delay / 1000} ثانیه برای تلاش دوباره...");
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ خطا در تلاش {attempt}: {ex.Message}");
                    
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"⏳ صبر {delay / 1000} ثانیه برای تلاش دوباره...");
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
