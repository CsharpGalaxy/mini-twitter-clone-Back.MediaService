using Microsoft.EntityFrameworkCore;
using MiniTwitter.MediaService.Data;
using MiniTwitter.MediaService.Models;
using MiniTwitter.MediaService.Services.Implement;
using MiniTwitter.MediaService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
//  ثبت سرویس‌ها
// ----------------------

// کنترلرها
builder.Services.AddControllers();

// Swagger / OpenAPI برای تست و مستندات
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext با Postgres
builder.Services.AddDbContext<MediaDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"🔍 Connection String: {connStr}");
    options.UseSqlServer(connStr);
    ////options.UseNpgsql(connStr);
});

// تنظیمات S3 (MinIO / Arvan / ...)
builder.Services.Configure<S3Configuration>(
    builder.Configuration.GetSection("S3Configuration"));

// سرویس initialization S3
builder.Services.AddScoped<S3InitializationService>();

// سرویس ذخیره‌سازی فایل روی S3
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();
//builder.Services.AddScoped<IFileStorageService, DummyFileStorageService>();


var app = builder.Build();

// ----------------------
//  مایگریشن دیتابیس
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    db.Database.Migrate();
    
    // Initialize S3 Bucket
    try
    {
        var s3Init = scope.ServiceProvider.GetRequiredService<S3InitializationService>();
        await s3Init.InitializeAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ خطا در مقداردهی S3: {ex.Message}");
        // برنامه ادامه می‌یابد حتی اگر S3 آماده نباشد
    }
}

// ----------------------
//  Middleware ها
// ----------------------

// Swagger UI در Development و Docker
if (app.Environment.IsDevelopment() ||
    app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
