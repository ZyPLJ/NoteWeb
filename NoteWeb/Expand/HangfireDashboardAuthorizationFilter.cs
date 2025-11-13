using Hangfire.Dashboard;

namespace NoteWeb.Expand;

// Filters/HangfireDashboardAuthorizationFilter.cs
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _configuration;

    public HangfireDashboardAuthorizationFilter(IConfiguration configuration)
    {
        _configuration = configuration;
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
        var allowedIps = _configuration.GetSection("Hangfire:AllowedIPs").Get<string[]>() ?? Array.Empty<string>();

        if (allowedIps.Contains(clientIp))
        {
            return true;
        }

        return false;
    }
}