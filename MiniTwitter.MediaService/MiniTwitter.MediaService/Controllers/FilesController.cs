using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwitter.MediaService.Data;
using MiniTwitter.MediaService.Models;
using MiniTwitter.MediaService.Services.Interface;

namespace MiniTwitter.MediaService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;
        private readonly MediaDbContext _db;

        public FilesController(IFileStorageService fileStorage, MediaDbContext db)
        {
            _fileStorage = fileStorage;
            _db = db;
        }

        // این اکشن برای سرویس‌های دیگه است که آپلود کنن
        // POST api/files
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string uploaderId, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var fileId = await _fileStorage.UploadAsync(file, uploaderId, ct);

            // برگردوندن id که سرویس‌های دیگه ذخیره کنن
            return Ok(new { FileId = fileId });
        }

        // گرفتن اطلاعات فایل (مثلاً برای نمایش در پروفایل)
        // GET api/files/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FileRecord>> GetInfo(Guid id, CancellationToken ct)
        {
            var record = await _db.Files.FirstOrDefaultAsync(f => f.Id == id, ct);
            if (record == null) return NotFound();

            return Ok(record);
        }

        // دانلود خود فایل (اختیاری – شاید سرویس‌های داخلی استفاده کنن)
        // GET api/files/{id}/download
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id, CancellationToken ct)
        {
            var stream = await _fileStorage.DownloadAsync(id.ToString(), ct);
            // برای سادگی، نوع رو generic می‌ذاریم
            return File(stream, "application/octet-stream", $"file-{id}");
        }

        // DELETE api/files/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _fileStorage.DeleteAsync(id.ToString(), ct);
            return NoContent();
        }
    }

}

