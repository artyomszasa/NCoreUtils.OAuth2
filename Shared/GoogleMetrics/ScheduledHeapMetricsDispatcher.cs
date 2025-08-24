using System.Runtime.CompilerServices;
using NCoreUtils.Google.Cloud.Monitoring;

namespace NCoreUtils.AspNetCore.OAuth2;

internal class ScheduledHeapMetricsDispatcher(
    ILogger<ScheduledHeapMetricsDispatcher> logger,
    IMonitoringV3Api api,
    string projectId,
    MonitoredResource resource,
    TimeSpan? delay = default)
{
    private static Metric HeapSize { get; } = new("custom.googleapis.com/dotnet/heapSize");

    private static Metric MemoryLoad { get; } = new("custom.googleapis.com/dotnet/memoryLoad");

    private static Metric TotalCommitted { get; } = new("custom.googleapis.com/dotnet/totalCommitted");

    private static Metric TotalAvailable { get; } = new("custom.googleapis.com/dotnet/totalAvailable");

    private static Metric Fragmented { get; } = new("custom.googleapis.com/dotnet/fragmented");

    private static async Task<string> GetMetadata(IHttpClientFactory httpClientFactory, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Metadata-Flavor", "Google");
        using var client = httpClientFactory.CreateClient();
        using var response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<(MonitoredResource Resource, string ProjecctId)> FetchCurrentResourceDataAsync(
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        var projectId = await GetMetadata(httpClientFactory, "http://metadata.google.internal/computeMetadata/v1/project/project-id", cancellationToken)
            ?? throw new InvalidOperationException("No project id found.");
        var zone = await GetMetadata(httpClientFactory, "http://metadata.google.internal/computeMetadata/v1/instance/zone", cancellationToken)
            ?? throw new InvalidOperationException("No zone/location found.");
        var location = zone.LastIndexOf('/') switch
        {
            -1 => zone,
            var ix => zone[(ix + 1)..]
        };
        var clusterName = await GetMetadata(httpClientFactory, "http://metadata.google.internal/computeMetadata/v1/instance/attributes/cluster-name", cancellationToken)
            ?? throw new InvalidOperationException("No cluster name found.");
        var namespaceName = Environment.GetEnvironmentVariable("K8S_POD_NAMESPACE") ?? throw new InvalidOperationException("No pod namespace provided.");
        var podName = Environment.GetEnvironmentVariable("K8S_POD_NAME") ?? throw new InvalidOperationException("No pod name provided.");
        var containerName = Environment.GetEnvironmentVariable("K8S_CONTAINER_NAME") ?? throw new InvalidOperationException("No container name provided.");
        var resource = MonitoredResource.K8sContainer(projectId, location, clusterName, namespaceName ?? string.Empty, podName ?? string.Empty, containerName ?? string.Empty);
        return (resource, projectId);
    }

    private readonly TimeSpan Delay = delay ?? TimeSpan.FromSeconds(15);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TimeSeries UpdateTimeSeries(TimeSeries ts, Point point, Metric metric, DateTimeOffset now, long value) => ts.Update(
        metric: metric,
        resource: resource,
        metricKind: MetricKinds.Gauge,
        points: point.Update(now, value)
    );

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Point heapSizePoint = new(default, default);
        Point memoryLoadPoint = new(default, default);
        Point totalCommittedPoint = new(default, default);
        Point totalAvailablePoint = new(default, default);
        Point fragmentedPoint = new(default, default);
        TimeSeries heapSizeEntry = new(default!, default!, default!, default(Points));
        TimeSeries memoryLoadEntry = new(default!, default!, default!, default(Points));
        TimeSeries totalCommittedEntry = new(default!, default!, default!, default(Points));
        TimeSeries totalAvailableEntry = new(default!, default!, default!, default(Points));
        TimeSeries fragmentedEntry = new(default!, default!, default!, default(Points));
        TimeSeries[] entries = [heapSizeEntry, memoryLoadEntry, totalCommittedEntry, totalAvailableEntry, fragmentedEntry];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var info = GC.GetGCMemoryInfo();
                long heapSize = info.HeapSizeBytes;
                long memoryLoad = info.MemoryLoadBytes;
                long totalCommitted = info.TotalCommittedBytes;
                long totalAvailable = info.TotalAvailableMemoryBytes;
                long fragmented = info.FragmentedBytes;
                // info.PinnedObjectsCount;
                try
                {
                    var now = DateTimeOffset.Now;
                    UpdateTimeSeries(heapSizeEntry, heapSizePoint, HeapSize, now, heapSize);
                    UpdateTimeSeries(memoryLoadEntry, memoryLoadPoint, MemoryLoad, now, memoryLoad);
                    UpdateTimeSeries(totalCommittedEntry, totalCommittedPoint, TotalCommitted, now, totalCommitted);
                    UpdateTimeSeries(totalAvailableEntry, totalAvailablePoint, TotalAvailable, now, totalAvailable);
                    UpdateTimeSeries(fragmentedEntry, fragmentedPoint, Fragmented, now, fragmented);
                    await api.CreateTimeSeriesAsync(projectId, entries, CancellationToken.None).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { /* noop */ }
                catch (Exception exn)
                {
                    logger.LogError(exn, "failed to dispatch heap metrics.");
                }
                await Task.Delay(Delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { /* noop */ }
            catch (Exception exn)
            {
                logger.LogError(exn, "failed to dispatch heap metrics.");
            }
        }
    }
}