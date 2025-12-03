using Microsoft.EntityFrameworkCore;
using MiniTwitter.MediaService.Models;

namespace MiniTwitter.MediaService.Data
{
    public class MediaDbContext : DbContext
    {
        public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
        {
        }
        public DbSet<FileRecord> Files { get; set; } = null!;
    }
}
