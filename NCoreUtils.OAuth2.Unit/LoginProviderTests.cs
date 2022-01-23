using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.AspNetCore;
using NCoreUtils.AspNetCore.Proto;
using NCoreUtils.OAuth2.Internal;
using Xunit;

namespace NCoreUtils.OAuth2.Unit;

public partial class LoginProviderTests
{
    private const string ExtensionGrantType = "https://example.com/auth";

    private const string User1Id = "1000";

    private const string User2Id = "1001";

    private const string Password = "xasdxasd";

    private const string Passcode1 = "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQ=";

    private const string Passcode2 = "TG8yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQ=";

    private const string Issuer = "https://example.com";

    private const string Username = "TEST";

    private const string Email1 = "test1@example.com";
    private const string Email2 = "test2@example.com";

    private static readonly ScopeCollection TestScopes = new(new[] { "test0", "test1" });

    private sealed class Startup
    {
        [SuppressMessage("Performance", "CA1822")]
        public void ConfigureServices(IServiceCollection services)
        {
            var loginProvider = new LoginProvider(new[]
            {
                new User(User1Id, Password, Passcode1, Issuer, Username, Email1, TestScopes),
                new User(User2Id, Password, Passcode2, Issuer, Username, Email2, default)
            });
            services
                .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .AddSingleton<ILoginProvider>(loginProvider)
                .AddRouting();
        }

        [SuppressMessage("Performance", "CA1822")]
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapLoginProvider();
                });
        }
    }

    [Fact]
    public async Task PasswordGrantTest()
    {
        // server side
        using var appFactory = new StandaloneWebApplicationFactory<Startup>();
        // client side
        var services = new ServiceCollection()
            .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup>(appFactory))
            .AddLoginProviderClient(new EndpointConfiguration { Endpoint = "http://localhost" })
            .BuildServiceProvider();
        try
        {
            var loginProvider = services.GetRequiredService<ILoginProvider>();
            {
                var identity = await loginProvider.PasswordGrantAsync(Email1, Password, TestScopes);
                Assert.NotNull(identity);
                Assert.Equal(User1Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email1, identity.Email);
                Assert.Equal(TestScopes, identity.Scopes);
            }
            {
                var identity = await loginProvider.PasswordGrantAsync(Email1, Password, default);
                Assert.NotNull(identity);
                Assert.Equal(User1Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email1, identity.Email);
                Assert.Equal(TestScopes, identity.Scopes);
            }
            {
                var identity = await loginProvider.PasswordGrantAsync(Email2, Password, TestScopes);
                Assert.NotNull(identity);
                Assert.Equal(User2Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email2, identity.Email);
                Assert.Equal(default, identity.Scopes);
            }
        }
        finally
        {
            (services as IDisposable)?.Dispose();
        }
    }

    [Fact]
    public async Task ExtensionGrantTest()
    {
        // server side
        using var appFactory = new StandaloneWebApplicationFactory<Startup>();
        // client side
        var services = new ServiceCollection()
            .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup>(appFactory))
            .AddLoginProviderClient(new EndpointConfiguration { Endpoint = "http://localhost" })
            .BuildServiceProvider();
        try
        {
            var loginProvider = services.GetRequiredService<ILoginProvider>();
            {
                var identity = await loginProvider.ExtensionGrantAsync(ExtensionGrantType, Passcode1, TestScopes);
                Assert.NotNull(identity);
                Assert.Equal(User1Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email1, identity.Email);
                Assert.Equal(TestScopes, identity.Scopes);
            }
            {
                var identity = await loginProvider.ExtensionGrantAsync(ExtensionGrantType, Passcode1, default);
                Assert.NotNull(identity);
                Assert.Equal(User1Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email1, identity.Email);
                Assert.Equal(TestScopes, identity.Scopes);
            }
            {
                var identity = await loginProvider.ExtensionGrantAsync(ExtensionGrantType, Passcode2, TestScopes);
                Assert.NotNull(identity);
                Assert.Equal(User2Id, identity!.Sub);
                Assert.Equal(Issuer, identity.Issuer);
                Assert.Equal(Username, identity.Name);
                Assert.Equal(Email2, identity.Email);
                Assert.Equal(default, identity.Scopes);
            }
        }
        finally
        {
            (services as IDisposable)?.Dispose();
        }
    }
}