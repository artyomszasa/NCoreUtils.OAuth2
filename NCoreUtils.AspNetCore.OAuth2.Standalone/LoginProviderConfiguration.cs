namespace NCoreUtils.AspNetCore.OAuth2;

public class LoginProviderConfiguration
{
    public const string DefaultHttpClientConfigurationName = "LoginProvider";

    public string? Host { get; }

    public IReadOnlyList<string> Hosts { get; }

    public string HttpClient { get; }

    public string Endpoint { get; }

    public string EndpointOrigin { get; }

    public string EndpointPath { get; }

    public LoginProviderConfiguration(string? host, IReadOnlyList<string> hosts, string httpClient, string endpoint)
    {
        Host = host;
        Hosts = hosts;
        HttpClient = httpClient;
        Endpoint = endpoint switch
        {
            null or "" => throw new ArgumentException("Endpoint must be a non-empty string.", nameof(endpoint)),
            var ep => ep
        };
        var uri = new Uri(Endpoint, UriKind.Absolute);
        EndpointOrigin = $"{uri.Scheme}://{uri.Host}";
        EndpointPath = uri.AbsolutePath;
    }

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