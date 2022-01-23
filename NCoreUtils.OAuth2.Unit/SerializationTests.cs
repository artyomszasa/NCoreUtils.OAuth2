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

        private static readonly ScopeCollection TestScopes = new(new [] { "test01", "test02" });

        [Fact]
        public void LoginIdentityJsonSerialization()
        {
            var id0 = new LoginIdentity(Sub, Issuer, Username, Email, TestScopes);
            var typeInfo = LoginProviderSerializationContext.Default.LoginIdentity;
            var json = JsonSerializer.Serialize(id0, typeInfo);
            var id1 = JsonSerializer.Deserialize(json, typeInfo);
            Assert.Equal(id0, id1);
        }

        [Fact]
        public void AccessTokenResponseJsonSerialization()
        {
            var resp0 = new AccessTokenResponse(Token, "bearer", TimeSpan.FromMinutes(15), Token, TestScopes);
            var typeInfo = TokenServiceJsonSerializerContext.Default.AccessTokenResponse;
            var json = JsonSerializer.Serialize(resp0, typeInfo);
            var resp1 = JsonSerializer.Deserialize(json, typeInfo);
            Assert.Equal(resp0, resp1);
        }

        [Fact]
        public void IntrospectionResponseJsonSerialization()
        {
            var resp0 = new IntrospectionResponse(true, TestScopes, "1", Email, Username, "bearer", DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), Sub, Issuer);
            var typeInfo = TokenServiceJsonSerializerContext.Default.IntrospectionResponse;
            var json = JsonSerializer.Serialize(resp0, typeInfo);
            var resp1 = JsonSerializer.Deserialize(json, typeInfo);
            Assert.Equal(resp0, resp1);
        }

        [Fact]
        public void ErrorResponseJsonSerialization()
        {
            var resp0 = new ErrorResponse("error_code", "error_description");
            var typeInfo = TokenServiceJsonSerializerContext.Default.ErrorResponse;
            var json = JsonSerializer.Serialize(resp0, typeInfo);
            var resp1 = JsonSerializer.Deserialize(json, typeInfo);
            Assert.Equal(resp0, resp1);
        }

        [Fact]
        public void ScopeCollectionDefaultValueSerialization()
        {
            {
                var obj = new AccessTokenResponse(Token, "bearer", TimeSpan.FromMinutes(15), default, default);
                var typeInfo = TokenServiceJsonSerializerContext.Default.AccessTokenResponse;
                var json = JsonSerializer.Serialize(obj, typeInfo);
                Assert.DoesNotContain("scope", json);
            }
            {
                var obj = new IntrospectionResponse(true, default, "1", Email, Username, "bearer", DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), DateTimeOffset.Now.Normalize(), Sub, Issuer);
                var typeInfo = TokenServiceJsonSerializerContext.Default.IntrospectionResponse;
                var json = JsonSerializer.Serialize(obj, typeInfo);
                Assert.DoesNotContain("scope", json);
            }
            {
                var obj = new LoginIdentity(Sub, Issuer, Username, Email, default);
                var typeInfo = LoginProviderSerializationContext.Default.LoginIdentity;
                var json = JsonSerializer.Serialize(obj, typeInfo);
                Assert.DoesNotContain("scope", json);
            }
        }
    }
}