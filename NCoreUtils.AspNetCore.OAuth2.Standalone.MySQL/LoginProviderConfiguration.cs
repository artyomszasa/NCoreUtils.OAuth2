using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class LoginProviderConfiguration : IEndpointConfiguration
    {
        public string? Host { get; set; }

        public List<string> Hosts { get; set; } = new List<string>();

        public string HttpClient { get; set; } = "LoginProvider";

        public string Endpoint { get; set; } = string.Empty;

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
}