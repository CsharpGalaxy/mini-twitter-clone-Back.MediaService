namespace MiniTwitter.MediaService.Services.Interface
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, string uploaderId, CancellationToken ct = default);
        Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default);
        Task DeleteAsync(string fileId, CancellationToken ct = default);
    }
}
