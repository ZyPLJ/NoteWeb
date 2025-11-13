using Hangfire.Dashboard;

namespace NoteWeb.Expand;

// Filters/HangfireDashboardAuthorizationFilter.cs
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HangfireDashboardAuthorizationFilter> _logger;

    public HangfireDashboardAuthorizationFilter(IConfiguration configuration,
        ILogger<HangfireDashboardAuthorizationFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 开发环境允许访问
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return true;
        }

        // 检查是否在白名单IP中
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        _logger.LogInformation("Client IP: {0}", clientIp);

        var allowedIps = _configuration.GetSection("Hangfire:AllowedIPs").Get<string[]>() ?? Array.Empty<string>();

        if (allowedIps.Contains(clientIp))
        {
            return true;
        }

        return false;
    }
}