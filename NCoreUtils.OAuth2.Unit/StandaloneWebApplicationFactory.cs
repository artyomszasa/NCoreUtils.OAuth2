using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace NCoreUtils.OAuth2.Unit
{
    public class StandaloneWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
            => new HostBuilder()
                .ConfigureWebHost(b =>
                {
                    b.UseStartup<TStartup>();
                });
    }
}