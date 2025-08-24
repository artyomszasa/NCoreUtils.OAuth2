using NCoreUtils.Google.Cloud.Monitoring;

namespace NCoreUtils.AspNetCore.OAuth2;

internal class MetricsDispatcherService(
    ILogger<ScheduledHeapMetricsDispatcher> dispatcherLogger,
    ILogger<MetricsDispatcherService> logger,
    IMonitoringV3Api api,
    IHttpClientFactory httpClientFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MonitoredResource resource;
        string projectId;
        try
        {
            (resource, projectId) = await ScheduledHeapMetricsDispatcher
                .FetchCurrentResourceDataAsync(httpClientFactory, stoppingToken)
                .ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            logger.LogError(exn, "Failed to fetch monitored resource.");
            return;
        }
        await new ScheduledHeapMetricsDispatcher(
            dispatcherLogger,
            api,
            projectId: projectId,
            resource: resource,
            delay: TimeSpan.FromSeconds(40)
        ).RunAsync(stoppingToken).ConfigureAwait(false);
    }
}