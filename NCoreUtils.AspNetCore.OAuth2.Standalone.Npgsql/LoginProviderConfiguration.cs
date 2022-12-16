using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class LoginProviderConfiguration : IEndpointConfiguration
    {
        public const string DefaultHttpClientConfigurationName = "LoginProvider";

        public string? Host { get; }

        public IReadOnlyList<string>? Hosts { get; }

        public string HttpClient { get; }

        public string Endpoint { get; }

        public string? Path { get; }

        public LoginProviderConfiguration(string? host, IReadOnlyList<string>? hosts, string? httpClient, string endpoint, string? path)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new System.ArgumentException($"'{nameof(endpoint)}' cannot be null or whitespace.", nameof(endpoint));
            }
            Host = host;
            Hosts = hosts;
            HttpClient = httpClient ?? DefaultHttpClientConfigurationName;
            Endpoint = endpoint;
            Path = path;
        }

        public IEnumerable<string> GetAllHosts()
        {
            if (!string.IsNullOrEmpty(Host))
            {
                yield return Host;
            }
            if (Hosts is not null)
            {
                foreach (var host in Hosts)
                {
                    yield return host;
                }
            }
        }

        public bool Matches(HttpRequest request)
            => GetAllHosts().Contains(request.Host.Host);
    }
}