using Ganss.Xss;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteWeb.Entity;
using NoteWeb.Entity.Model;
using NoteWeb.Expand;
using NoteWeb.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); // 移除 EventLogProvider
builder.Logging.AddConsole(); // 改为控制台日志

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.WebHost.UseUrls("http://*:1556");

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        opt => opt.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("http://localhost:1556/", "https://localhost:1556/api"));
});

builder.Services.AddScoped<TempFilterService>();
builder.Services.AddScoped<WordCloudService>();

builder.Services.AddOptions();
builder.Services.AddRateLimit(builder.Configuration);

builder.Services.AddHangfire(config =>
    config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("NoteWeb API"); // 设置标题
    });
}

// 告诉 ASP.NET Core 使用 Nginx 转发的头部
var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// ⚠️ 默认只信任本机代理（127.0.0.1），但 Docker 内部的宿主机可能不是127，而是172网段
// 所以建议明确允许所有代理来源（或指定你的宿主机网段）
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeaderOptions);

// 配置定时任务
// 配置 Hangfire 仪表板
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "任务调度监控",
    Authorization =
    [
        new HangfireDashboardAuthorizationFilter(app.Configuration,
            app.Services.GetRequiredService<ILogger<HangfireDashboardAuthorizationFilter>>())
    ]
});

// 每天0:00, 4:00, 8:00, 12:00, 16:00, 20:00执行
RecurringJob.AddOrUpdate<WordCloudService>(
    "generate-wordcloud",
    service => service.GenerateWordCloud(),
    "0 0,4,8,12,16,20 * * *");

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

// 启用静态文件服务
app.UseStaticFiles();

// 启用限流中间件
app.UseRateLimit();

// get all notes 按时间降序 取前150条
app.MapGet("/api/notes",
    async (MyDbContext db) =>
    {
        return await db.Notes.OrderByDescending(c => c.CreatedAt)
            .Take(150)
            .Select(a => a.Content)
            .ToListAsync();
    });

// add a new note
app.MapPost("/api/notes",
    async (MyDbContext db, TempFilterService filter, [FromBody] NoteDto dto) =>
    {
        // Content 不能超过30个中文字符
        if (dto.Content.Length > 30)
        {
            return Results.Json(new ApiResponse
            {
                StatusCode = 400,
                Successful = false,
                Message = "便签内容不能超过30个字"
            });
        }

        if (filter.CheckBadWord(dto.Content))
        {
            return Results.Json(new ApiResponse
            {
                StatusCode = 400,
                Successful = false,
                Message = "便签内容包含不良内容，请修改后重试"
            });
        }

        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Remove("img");
        sanitizer.AllowedTags.Remove("a");

        Note note = new Note
        {
            Content = sanitizer.Sanitize(dto.Content),
            CreatedAt = DateTime.UtcNow
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();
        return Results.Json(new ApiResponse
        {
            Message = "新增便签成功"
        });
    });

app.Run();

class NoteDto
{
    public string Content { get; set; } = string.Empty;
}

class ApiResponse
{
    public int StatusCode { get; set; } = 200;
    public bool Successful { get; set; } = true;
    public string? Message { get; set; } = "操作成功";
    public object? Data { get; set; }
}