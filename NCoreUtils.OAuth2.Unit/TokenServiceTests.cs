using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.AspNetCore;
using NCoreUtils.AspNetCore.Proto;
using NCoreUtils.OAuth2.Internal;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    public partial class TokenServiceTests
    {
        private sealed class Startup<TTokenEncryption> where TTokenEncryption : class, ITokenEncryption
        {
            public void ConfigureServices(IServiceCollection services)
            {
                var loginProvider = new LoginProvider(new[]
                {
                    new User(UserId, Password, Passcode, Issuer, Username, Email, TestScopes)
                });
                services
                    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace))
                    .AddSingleton<ILoginProvider>(loginProvider)
                    .AddSingleton<RijndaelTokenEncryptionConfiguration>(new RijndaelTokenEncryptionConfiguration { IV = "fRhAu1dtoC9cIQf4+kF68A==", Key = "c0eLpWb+GZAb0lf3chxWsU16pK4r4a5gZEWxpIdqDHg=" })
                    .AddTokenService<TTokenEncryption, TokenRepository>(new TokenServiceConfiguration
                    {
                        RefreshTokenExpiry = TimeSpan.FromHours(240),
                        AccessTokenExpiry = TimeSpan.FromMinutes(15),
                    }, tokenRepositoryLifetime: ServiceLifetime.Singleton)
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

        private static readonly ScopeCollection TestScopes = new ScopeCollection(new[] { "test" });

        [Fact]
        public async Task NoopPasswordGrantTest()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<NoopTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<NoopTokenEncryption>>(appFactory))
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
        public async Task NoopExtensionGrantTest()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<NoopTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<NoopTokenEncryption>>(appFactory))
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
        [Fact]
        public async Task RijndaelPasswordGrantTest()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
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
        public async Task RijndaelExtensionGrantTest()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
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
        [Fact]
        public async Task InvalidToken()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                await Assert.ThrowsAsync<RemoteAccessDeniedException>(async () => await tokenService.IntrospectAsync(""));
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public async Task ExpiredAccessToken()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                await Assert.ThrowsAsync<RemoteAccessDeniedException>(async () => await tokenService.IntrospectAsync("DAAAAGFjY2Vzc190b2tlbgQAAAAxMDAwEwAAAGh0dHBzOi8vZXhhbXBsZS5jb20QAAAAdGVzdEBleGFtcGxlLmNvbQQAAABURVNUAQAAAAQAAAB0ZXN0gHCA6Xt62AiAVg/ReXrYCA=="));
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }
        [Fact]
        public async Task InvalidCredentials()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                await Assert.ThrowsAsync<RemoteInvalidCredentialsException>(async () => await tokenService.ExtensionGrantAsync("invalid cred", Passcode, TestScopes));
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public async Task InvalidUsernameOrPassword()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                await Assert.ThrowsAsync<RemoteInvalidCredentialsException>(async () =>  await tokenService.PasswordGrantAsync("notvalid@domain.tld", "", TestScopes));
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public async Task InvalidRefreshToken()
        {
            // server side
            using var appFactory = new StandaloneWebApplicationFactory<Startup<RijndaelTokenEncryption>>();
            // client side
            var services = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(new TestHttpClientFactory<Startup<RijndaelTokenEncryption>>(appFactory))
                .AddTokenServiceClient(new EndpointConfiguration { Endpoint = "http://localhost" })
                .BuildServiceProvider();
            try
            {
                var tokenService = services.GetRequiredService<ITokenService>();
                await Assert.ThrowsAsync<RemoteAccessDeniedException>(async () => await tokenService.RefreshTokenAsync("", TestScopes));
            }
            finally
            {
                (services as IDisposable)?.Dispose();
            }
        }
    }
}