using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Kích hoạt CORS
app.UseCors();

// Phục vụ các tệp tĩnh từ wwwroot
app.UseStaticFiles();

// Định tuyến API
app.MapControllers();

// Định tuyến cho các trang HTML
app.MapGet("/about", async context =>
{
    await context.Response.SendFileAsync("wwwroot/about.html");
});

app.MapGet("/auctions", async context =>
{
    await context.Response.SendFileAsync("wwwroot/auctions.html");
});

// Định tuyến fallback: nếu không tìm thấy route, trả về index.html
app.MapFallbackToFile("index.html");

app.Run();
