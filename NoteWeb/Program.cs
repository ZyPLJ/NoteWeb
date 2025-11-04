using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteWeb.Entity;
using NoteWeb.Entity.Model;
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
    async (MyDbContext db, [FromBody] NoteDto dto) =>
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

        Note note = new Note
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        return Results.Json(new ApiResponse
        {
            Message = "新增便签成功"
        });
    });

app.UseStaticFiles();

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