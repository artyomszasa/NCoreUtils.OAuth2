using System;
using System.Text.Json;
using NCoreUtils.OAuth2.Internal;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    public class SerializationTests
    {
        private const string Sub = "12345";

        private const string Issuer = "https://example.com";

        private const string Username = "user";

        private const string Email = "user@example.com";

        private const string Token = "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQ=";

        private static readonly ScopeCollection TestScopes = new ScopeCollection(new [] { "test" });

        [Fact]
        public void LoginIdentityJsonSerialization()
        {
            var id0 = new LoginIdentity(Sub, Issuer, Username, Email, TestScopes);
            var opts = new JsonSerializerOptions { Converters = { LoginIdentityConverter.Instance } };
            var json = JsonSerializer.Serialize(id0, opts);
            var id1 = JsonSerializer.Deserialize<LoginIdentity>(json, opts);
            Assert.Equal(id0, id1);
        }

        [Fact]
        public void AccessTokenResponseJsonSerialization()
        {
            var resp0 = new AccessTokenResponse(Token, "bearer", TimeSpan.FromMinutes(15), Token, TestScopes);
            var opts = new JsonSerializerOptions { Converters = { AccessTokenResponseConverter.Instance } };
            var json = JsonSerializer.Serialize(resp0, opts);
            var resp1 = JsonSerializer.Deserialize<AccessTokenResponse>(json, opts);
            Assert.Equal(resp0, resp1);
        }

        [Fact]
        public void IntrospectionResponseJsonSerialization()
        {
            var resp0 = new IntrospectionResponse(true, TestScopes, "1", Email, Username, "bearer", DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), Sub, Issuer);
            var opts = new JsonSerializerOptions { Converters = { IntrospectionResponseConverter.Instance } };
            var json = JsonSerializer.Serialize(resp0, opts);
            var resp1 = JsonSerializer.Deserialize<IntrospectionResponse>(json, opts);
            Assert.Equal(resp0, resp1);
        }

        [Fact]
        public void ErrorResponseJsonSerialization()
        {
            var resp0 = new ErrorResponse("error_code", "error_description");
            var opts = new JsonSerializerOptions { Converters = { ErrorResponseConverter.Instance } };
            var json = JsonSerializer.Serialize(resp0, opts);
            var resp1 = JsonSerializer.Deserialize<ErrorResponse>(json, opts);
            Assert.Equal(resp0, resp1);
        }
    }
}