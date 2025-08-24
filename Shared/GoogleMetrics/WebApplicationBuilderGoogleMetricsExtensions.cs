using NCoreUtils.Google;

namespace NCoreUtils.AspNetCore.OAuth2;

internal static class WebApplicationBuilderGoogleMetricsExtensions
{
    public static WebApplicationBuilder AddGoogleHeapMonitoring(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddGoogleCloudMonitoringClient(default(string))
            .AddHostedService<MetricsDispatcherService>();
        return builder;
    }

    public static WebApplicationBuilder AddGoogleHeapMonitoring(this WebApplicationBuilder builder, ServiceAccountCredentialData googleCredentials)
    {
        builder.Services
            .AddGoogleCloudMonitoringClient(googleCredentials)
            .AddHostedService<MetricsDispatcherService>();
        return builder;
    }
}