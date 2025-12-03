using Microsoft.EntityFrameworkCore;
using MiniTwitter.MediaService.Data;
using MiniTwitter.MediaService.Models;
using MiniTwitter.MediaService.Services.Implement;
using MiniTwitter.MediaService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// PostgreSQL
builder.Services.AddDbContext<MediaDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connStr);
});

// S3 config
builder.Services.Configure<S3Configuration>(
    builder.Configuration.GetSection("S3Configuration"));

// S3 file storage service
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    db.Database.Migrate();
}

app.Run();
