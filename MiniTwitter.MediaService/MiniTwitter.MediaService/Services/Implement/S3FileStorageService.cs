using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniTwitter.MediaService.Data;
using MiniTwitter.MediaService.Models;
using MiniTwitter.MediaService.Services.Interface;

namespace MiniTwitter.MediaService.Services.Implement
{
    public class S3FileStorageService: IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Configuration _config;
        private readonly MediaDbContext _db;

        public S3FileStorageService(
            IOptions<S3Configuration> configOptions,
            MediaDbContext dbContext)
        {
            _config = configOptions.Value;
            _db = dbContext;

            var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);

            var s3Config = new AmazonS3Config
            {
                ServiceURL = _config.ServiceURL,
                ForcePathStyle = true,
                RegionEndpoint = RegionEndpoint.GetBySystemName(_config.Region)
            };

            _s3Client = new AmazonS3Client(credentials, s3Config);
        }

        private string GenerateS3Url(string key)
        {
            return $"{_config.ServiceURL}/{_config.BucketName}/{key}";
        }

        public async Task<string> UploadAsync(IFormFile file, string uploaderId, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            var key = $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _config.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.Private
            };

            await _s3Client.PutObjectAsync(request, ct);

            var url = GenerateS3Url(key);

            // ذخیره در دیتابیس
            var record = new FileRecord
            {
                FileName = file.FileName,
                Url = url,
                ContentType = file.ContentType,
                Size = file.Length,
                UploadedBy = uploaderId
            };

            _db.Files.Add(record);
            await _db.SaveChangesAsync(ct);

            // دیگه از این به بعد سرویس‌های دیگه می‌تونن با Id این رکورد کار کنن
            return record.Id.ToString();
        }

        public async Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default)
        {
            if (!Guid.TryParse(fileId, out var guid))
                throw new ArgumentException("Invalid file id", nameof(fileId));

            var record = await _db.Files.FirstOrDefaultAsync(f => f.Id == guid, ct);
            if (record == null)
                throw new FileNotFoundException("File record not found");

            // از روی URL، key رو دربیاریم
            var prefix = $"{_config.ServiceURL}/{_config.BucketName}/";
            if (!record.Url.StartsWith(prefix))
                throw new InvalidOperationException("File URL is not valid for this config");

            var key = record.Url.Substring(prefix.Length);

            var request = new GetObjectRequest
            {
                BucketName = _config.BucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request, ct);

            var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms, ct);
            ms.Position = 0;
            return ms;
        }

        public async Task DeleteAsync(string fileId, CancellationToken ct = default)
        {
            if (!Guid.TryParse(fileId, out var guid))
                throw new ArgumentException("Invalid file id", nameof(fileId));

            var record = await _db.Files.FirstOrDefaultAsync(f => f.Id == guid, ct);
            if (record == null)
                return;

            var prefix = $"{_config.ServiceURL}/{_config.BucketName}/";
            var key = record.Url.Substring(prefix.Length);

            var request = new DeleteObjectRequest
            {
                BucketName = _config.BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, ct);

            _db.Files.Remove(record);
            await _db.SaveChangesAsync(ct);
        }
    }

}

