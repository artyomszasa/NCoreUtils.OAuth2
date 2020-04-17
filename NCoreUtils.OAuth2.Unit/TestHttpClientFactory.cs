using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NCoreUtils.OAuth2.Unit
{
    public class TestHttpClientFactory<TStartup> : IHttpClientFactory
        where TStartup : class
    {
        private readonly WebApplicationFactory<TStartup> _webApplicationFactory;

        public TestHttpClientFactory(WebApplicationFactory<TStartup> webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory ?? throw new ArgumentNullException(nameof(webApplicationFactory));

        public HttpClient CreateClient(string name) => _webApplicationFactory.CreateClient();
    }
}