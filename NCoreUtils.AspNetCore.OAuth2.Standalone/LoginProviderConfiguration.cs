namespace NCoreUtils.AspNetCore.OAuth2;

public class LoginProviderConfiguration(string? host, IReadOnlyList<string> hosts, string httpClient, string endpoint)
{
    public const string DefaultHttpClientConfigurationName = "LoginProvider";

    public string? Host { get; } = host;

    public IReadOnlyList<string> Hosts { get; } = hosts;

    public string HttpClient { get; } = httpClient;

    public string Endpoint { get; } = endpoint switch
    {
        null or "" => throw new ArgumentException("Endpoint must be a non-empty string.", nameof(endpoint)),
        var ep => ep
    };

    public IEnumerable<string> GetAllHosts()
    {
        if (!string.IsNullOrEmpty(Host))
        {
            yield return Host;
        }
        foreach (var host in Hosts)
        {
            yield return host;
        }
    }

    public bool Matches(HttpRequest request)
        => GetAllHosts().Contains(request.Host.Value);
}