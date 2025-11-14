using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NoteWeb.Entity;
using Sdcb.WordClouds;
using SkiaSharp;

namespace NoteWeb.Services;

public class WordCloudService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WordCloudService> _logger;

    public WordCloudService(IServiceProvider serviceProvider, ILogger<WordCloudService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task GenerateWordCloud()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var allNotes = await db.Notes.Select(n => n.Content).ToListAsync();
            var text = string.Join(" ", allNotes);

            var words = ProcessText(text)
                .GroupBy(word => word)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kv => kv.Value)
                .Take(150)
                .ToDictionary(kv => kv.Key, kv => kv.Value)
                .Select(kv => new WordScore(kv.Key, kv.Value));

            WordCloud wc = WordCloud.Create(new WordCloudOptions(1000, 1000, words));
            byte[] pngBytes = wc.ToSKBitmap().Encode(SKEncodedImageFormat.Png, 100).AsSpan().ToArray();

            string filePath = "wwwroot/wordcloud.png";

            await File.WriteAllBytesAsync(filePath, pngBytes);

            _logger.LogInformation($"词云图已更新: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成词云图时发生错误");
        }
    }

    private static IEnumerable<string> ProcessText(string text)
    {
        var words = Regex.Matches(text, @"[\u4e00-\u9fa5]+|[a-zA-Z]+")
            .Select(m => m.Value.ToLower())
            .Where(word => word.Length > 1);

        return words;
    }
}