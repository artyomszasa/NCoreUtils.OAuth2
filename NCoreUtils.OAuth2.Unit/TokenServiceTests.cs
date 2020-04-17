using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore;
using NCoreUtils.AspNetCore.Proto;
using NCoreUtils.OAuth2.Internal;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    public partial class TokenServiceTests
    {
        private sealed class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                var loginProvider = new LoginProvider(new []
                {
                    new User(UserId, Password, Passcode, Issuer, Username, Email, TestScopes)
                });
                services
                    .AddSingleton<ILoginProvider>(loginProvider)
                    .AddSingleton<ITokenRepository, TokenRepository>()
                    .AddSingleton<ITokenEncryption, NoopTokenEncryption>()
                    .AddSingleton<ITokenService, TokenService>()
                    .AddSingleton<ITokenServiceConfiguration>(new TokenServiceConfiguration
                    {
                        RefreshTokenExpiry = TimeSpan.FromHours(240),
                        AccessTokenExpiry = TimeSpan.FromMinutes(15)
                    })
                    .AddRouting();
            }

            public void Configure(IApplicationBuilder app)
            {
                app
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.MapTokenService();
                    });
            }
        }

        private const string ExtensionGrantType = "https://example.com/auth";

        private const string UserId = "1000";

        private const string Password = "xasdxasd";

        private const string Passcode = "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQ=";

        private const string Issuer = "https://example.com";

        private const string Username = "TEST";

        private const string Email = "test@example.com";

        private static readonly ScopeCollection TestScopes = new ScopeCollection(new [] { "test" });

        [Fact]
        public async Task PasswordGrantTest()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                var accessTokenResponse = await tokenService.PasswordGrantAsync(Email, Password, TestScopes);
                Assert.NotNull(accessTokenResponse.AccessToken);
                Assert.NotNull(accessTokenResponse.RefreshToken);
                var info = await tokenService.IntrospectAsync(accessTokenResponse.AccessToken);
                Assert.True(info.Active);
                Assert.Equal(UserId, info.Sub);
                var refreshToken = accessTokenResponse.RefreshToken!;
                accessTokenResponse = await tokenService.RefreshTokenAsync(refreshToken, TestScopes);
                Assert.NotNull(accessTokenResponse.AccessToken);
                Assert.Null(accessTokenResponse.RefreshToken);
                info = await tokenService.IntrospectAsync(accessTokenResponse.AccessToken);
                Assert.True(info.Active);
                Assert.Equal(UserId, info.Sub);
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
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                var accessTokenResponse = await tokenService.ExtensionGrantAsync(ExtensionGrantType, Passcode, TestScopes);
                Assert.NotNull(accessTokenResponse.AccessToken);
                Assert.NotNull(accessTokenResponse.RefreshToken);
                var info = await tokenService.IntrospectAsync(accessTokenResponse.AccessToken);
                Assert.True(info.Active);
                Assert.Equal(UserId, info.Sub);
                var refreshToken = accessTokenResponse.RefreshToken!;
                accessTokenResponse = await tokenService.RefreshTokenAsync(refreshToken, TestScopes);
                Assert.NotNull(accessTokenResponse.AccessToken);
                Assert.Null(accessTokenResponse.RefreshToken);
                info = await tokenService.IntrospectAsync(accessTokenResponse.AccessToken);
                Assert.True(info.Active);
                Assert.Equal(UserId, info.Sub);
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }
    }
}