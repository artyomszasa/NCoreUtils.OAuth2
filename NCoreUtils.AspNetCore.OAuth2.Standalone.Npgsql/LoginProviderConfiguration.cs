using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore.OAuth2;

public class LoginProviderConfiguration(
    string? host,
    IReadOnlyList<string>? hosts,
    string? httpClient,
    string endpoint,
    string? path)
    : IEndpointConfiguration
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AssertNeitherNullNorWhiteSpace(
        string argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
        return argument;
    }

    public const string DefaultHttpClientConfigurationName = "LoginProvider";

    public string? Host { get; } = host;

    public IReadOnlyList<string>? Hosts { get; } = hosts;

    public string HttpClient { get; } = httpClient ?? DefaultHttpClientConfigurationName;

    public string Endpoint { get; } = AssertNeitherNullNorWhiteSpace(endpoint);

    public string? Path { get; } = path;

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