using AspNetCoreRateLimit;

namespace NoteWeb.Expand;

public static class ConfigureRateLimit
{
    public static void AddRateLimit(this IServiceCollection services,IConfiguration conf)
    {
        services.Configure<IpRateLimitOptions>(conf.GetSection("IpRateLimiting"));
        // 使用内存缓存服务替换 Redis 缓存服务
        services.AddMemoryCache();
        // 使用内存限流
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }
    public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app)
    {
        app.UseIpRateLimiting();
        return app;
    }
}