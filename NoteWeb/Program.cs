using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteWeb.Entity;
using NoteWeb.Entity.Model;
using NoteWeb.Expand;
using NoteWeb.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddOptions();
builder.Services.AddRateLimit(builder.Configuration);

builder.Services.AddMemoryCache();

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

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

//添加中间件
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});

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