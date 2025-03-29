namespace NCoreUtils.AspNetCore.OAuth2;

public class LoginProviderConfiguration
{
    public const string DefaultHttpClientConfigurationName = "LoginProvider";

    private HashSet<string> AllHosts { get; }

    public string? Host { get; }

    public IReadOnlyList<string> Hosts { get; }

    public string HttpClient { get; }

    public string Endpoint { get; }

    public string EndpointOrigin { get; }

    public string EndpointPath { get; }

    public LoginProviderConfiguration(string? host, IReadOnlyList<string> hosts, string httpClient, string endpoint)
    {
        var hosts0 = hosts ?? [];
        Host = host;
        Hosts = hosts0;
        HttpClient = httpClient;
        Endpoint = endpoint switch
        {
            null or "" => throw new ArgumentException("Endpoint must be a non-empty string.", nameof(endpoint)),
            var ep => ep
        };
        var uri = new Uri(Endpoint, UriKind.Absolute);
        EndpointOrigin = $"{uri.Scheme}://{uri.Host}";
        EndpointPath = uri.AbsolutePath;
        if (string.IsNullOrEmpty(host))
        {
            AllHosts = [..hosts0];
        }
        else
        {
            AllHosts = [host, ..hosts0];
        }
    }

    public bool Matches(HttpRequest request)
    {
        var host = request.Host;
        if (host.HasValue)
        {
            if (host.Value is string { Length: >0 } rawHost)
            {
                if (AllHosts.Contains(rawHost))
                {
                    return true;
                }
            }
            if (host.Host is string { Length: >0 } hostOnly)
            {
                if (AllHosts.Contains(hostOnly))
                {
                    return true;
                }
            }
        }
        return false;
    }
}