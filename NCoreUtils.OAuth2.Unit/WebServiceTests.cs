using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;
using NCoreUtils.OAuth2.WebService;
using Newtonsoft.Json;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace NCoreUtils.OAuth2.Unit
{
    [CollectionDefinition(nameof(NCoreUtils.OAuth2.Unit.WebServiceTests), DisableParallelization = true)]
    public class WebServiceTests : WebTestBase<Startup>
    {
        private const string TestEmail = "test@test.dbg";
        private const string TestPassword = "xasd";

        IServiceProvider _serviceProvider;

        int _userId;

        int _clientApplicationId;

        public WebServiceTests(WebApplicationFactory<Startup> factory)
            : base(factory)
        {
            factory.ClientOptions.AllowAutoRedirect = false;
        }

        void InitDb(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OAuth2DbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                var clientApplicationRepository = scope.ServiceProvider.GetRequiredService<IDataRepository<ClientApplication>>();
                var clientApplication = clientApplicationRepository.Persist(new ClientApplication
                {
                    Name = "Test domain",
                    Domains = new HashSet<Domain>(new []
                    {
                        new Domain { DomainName = "localhost" }
                    })
                });
                _clientApplicationId = clientApplication.Id;
                scope.ServiceProvider.GetRequiredService<CurrentClientApplication>().Id = clientApplication.Id;
                var permissionRepository = scope.ServiceProvider.GetRequiredService<IDataRepository<Permission>>();
                var readPermission = permissionRepository.Persist(new Permission
                {
                    ClientApplicationId = clientApplication.Id,
                    Name = "user.read"
                });
                var writePermission = permissionRepository.Persist(new Permission
                {
                    ClientApplicationId = clientApplication.Id,
                    Name = "user.write"
                });

                var pwritePermission = permissionRepository.Persist(new Permission
                {
                    ClientApplicationId = clientApplication.Id,
                    Name = "permission.write"
                });

                var userRepository = scope.ServiceProvider.GetRequiredService<IDataRepository<User>>();
                var user = userRepository.Persist(new User
                {
                    ClientApplication = clientApplication,
                    FamilyName = "TEST",
                    GivenName = "TEST",
                    Email = TestEmail,
                    Password = TestPassword,
                    Permissions = new HashSet<UserPermission>(new []
                    {
                        new UserPermission { PermissionId = readPermission.Id },
                        new UserPermission { PermissionId = writePermission.Id },
                        new UserPermission { PermissionId = pwritePermission.Id },
                    })
                });
                _userId = user.Id;
            }
        }


        protected override void InitializeServices(IServiceCollection services)
        {
            base.InitializeServices(services);
            services
                .ForceInMemoryDatabase<OAuth2DbContext>()
                .RemoveAll(typeof(IEncryptionProvider))
                .AddSingleton<IEncryptionProvider, DummyEncryption>()
                .AddServiceInterceptor(serviceProvider =>
                {
                    _serviceProvider = serviceProvider;
                    InitDb(serviceProvider);
                });
        }

        [Fact]
        public async Task InvalidGrant()
        {
            using (var client = CreateClient())
            {
                var uri = $"/token";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2ErrorResult>(json);
                    Assert.Equal(OAuth2Error.InvalidRequest, resp.Error);
                }
            }
        }

        [Fact]
        public async Task InvalidHost()
        {
            using (var client = CreateClient())
            {
                var uri = $"http://oauth2.debug/token?grant_type=password&username={Uri.EscapeDataString(TestEmail)}&password={Uri.EscapeDataString(TestPassword)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                        var json = await response.Content.ReadAsStringAsync();
                        var resp = JsonConvert.DeserializeObject<OAuth2ErrorResult>(json);
                        Assert.Equal(OAuth2Error.InvalidRequest, resp.Error);
                    }
                }
            }
        }

        [Fact]
        public async Task PasswordGrantRefreshAndQuery()
        {
            using (var client = CreateClient())
            {
                string accessToken;
                string refreshToken;
                var uri = $"/token?grant_type=password&username={Uri.EscapeDataString(TestEmail)}&password={Uri.EscapeDataString(TestPassword)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.NotNull(resp.RefreshToken);
                    accessToken = resp.AccessToken;
                    refreshToken = resp.RefreshToken;
                }

                uri = $"/token?grant_type=refresh_token&refresh_token={Uri.EscapeDataString(refreshToken)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.Null(resp.RefreshToken);
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, "/data/user"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        var json = await response.Content.ReadAsStringAsync();
                        var resp = JsonConvert.DeserializeObject<Rest.MappedUser[]>(json);
                        var rawCount = response.Headers.TryGetValues("X-Total-Count", out var values) ? values.FirstOrDefault() : null;
                        Assert.NotNull(rawCount);
                        var count = int.Parse(rawCount);
                        Assert.Equal(1, count);
                        Assert.Equal(count, resp.Length);
                        Assert.Contains(resp, u => u.Email == TestEmail);
                    }
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, $"/data/user/{_userId}"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        var json = await response.Content.ReadAsStringAsync();
                        var resp = JsonConvert.DeserializeObject<Rest.MappedUser>(json);
                        Assert.Equal(TestEmail, resp.Email);
                    }
                }

                // Revoke token
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<OAuth2DbContext>();
                    var rtoken = await dbContext.Set<RefreshToken>().FirstOrDefaultAsync();
                    rtoken.State = State.Deleted;
                    await dbContext.SaveChangesAsync();
                }

                uri = $"/token?grant_type=refresh_token&refresh_token={Uri.EscapeDataString(refreshToken)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2ErrorResult>(json);
                    Assert.Equal(OAuth2Error.InvalidGrant, resp.Error);
                }
            }
        }

        [Fact]
        public async Task CodeGrantRefreshAndQuery()
        {
            using (var client = CreateClient())
            {
                string accessToken;
                string refreshToken;
                var redirectUri = "http://test.debug/callback";
                var regex = new Regex("^\\?code=(.*)$", RegexOptions.Compiled);
                string authCode;

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/login"))
                {
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { OAuth2Parameters.Password, TestPassword },
                        { OAuth2Parameters.Username, TestEmail },
                        { OAuth2Parameters.RedirectUri, redirectUri }
                    });
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                        var location = response.Headers.TryGetValues("Location", out var values) ? values.FirstOrDefault() : null;
                        Assert.NotNull(location);
                        Assert.StartsWith(redirectUri, location);
                        var m = regex.Match(location.Substring(redirectUri.Length));
                        Assert.True(m.Success);
                        authCode = Uri.UnescapeDataString(m.Groups[1].Value);
                    }
                }

                var uri = $"/token?grant_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&code={Uri.EscapeDataString(authCode)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.NotNull(resp.RefreshToken);
                    accessToken = resp.AccessToken;
                    refreshToken = resp.RefreshToken;
                }

                uri = $"/token?grant_type=refresh_token&refresh_token={Uri.EscapeDataString(refreshToken)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.Null(resp.RefreshToken);
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, "/openid"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        var json = await response.Content.ReadAsStringAsync();
                        var resp = JsonConvert.DeserializeObject<OpenIdUserInfo>(json);
                        Assert.Equal(TestEmail, resp.Email);
                        Assert.Contains("user.read", resp.Scopes);
                        Assert.Contains("user.write", resp.Scopes);
                    }
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, "/openid"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Xearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    }
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, "/data/user"))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        var json = await response.Content.ReadAsStringAsync();
                        var resp = JsonConvert.DeserializeObject<Rest.MappedUser[]>(json);
                        var rawCount = response.Headers.TryGetValues("X-Total-Count", out var values) ? values.FirstOrDefault() : null;
                        Assert.NotNull(rawCount);
                        var count = int.Parse(rawCount);
                        Assert.Equal(1, count);
                        Assert.Equal(count, resp.Length);
                        Assert.Contains(resp, u => u.Email == TestEmail);
                    }
                }
            }
        }

        [Fact]
        public async Task PermissionManagement()
        {
            using (var client = CreateClient())
            {
                string accessToken;
                string refreshToken;
                int permissionId;
                var uri = $"/token?grant_type=password&username={Uri.EscapeDataString(TestEmail)}&password={Uri.EscapeDataString(TestPassword)}";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.NotNull(resp.RefreshToken);
                    accessToken = resp.AccessToken;
                    refreshToken = resp.RefreshToken;
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/data/permission"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(new Permission
                    {
                        Name = "test",
                        ClientApplicationId = _clientApplicationId
                    }), new UTF8Encoding(false), "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                        var location = response.Headers.TryGetValues("Location", out var values) ? values.FirstOrDefault() : null;
                        Assert.NotNull(location);
                        var m = Regex.Match(location, "[0-9]+$");
                        Assert.True(m.Success);
                        permissionId = int.Parse(m.Value);
                    }
                }

                using (var request = new HttpRequestMessage(HttpMethod.Put, $"/data/permission/{permissionId}"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(new Permission
                    {
                        Id = permissionId,
                        Name = "test",
                        Description = "TEST",
                        ClientApplicationId = _clientApplicationId
                    }), new UTF8Encoding(false), "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    }
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/data/permission"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(new Permission
                    {
                        Name = "test",
                        ClientApplicationId = _clientApplicationId + 1
                    }), new UTF8Encoding(false), "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                    }
                }

                uri = $"/token?grant_type=password&username={Uri.EscapeDataString(TestEmail)}&password={Uri.EscapeDataString(TestPassword)}&scope=user.read";
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<OAuth2Response>(json);
                    Assert.Equal("Bearer", resp.TokenType);
                    Assert.NotNull(resp.AccessToken);
                    Assert.NotNull(resp.RefreshToken);
                    accessToken = resp.AccessToken;
                    refreshToken = resp.RefreshToken;
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/data/permission"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(new Permission
                    {
                        Name = "test",
                        ClientApplicationId = _clientApplicationId
                    }), new UTF8Encoding(false), "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                    {
                        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                    }
                }
            }
        }
    }
}
