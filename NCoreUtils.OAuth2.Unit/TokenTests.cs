using System;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    public class TokenTests
    {
        [Fact]
        public void Reserialize()
        {
            var now = DateTimeOffset.Now;
            var token = new Token("access_token", "sub", "issuer", "email", "username", new string[] { "a" }, now, now);
            var buffer = new byte[16 * 1024];
            Assert.True(token.TryWriteTo(buffer.AsSpan(), out var size));
            Assert.True(Token.TryReadFrom(buffer.AsSpan().Slice(0, size), out var token1));
            Assert.Equal(token, token1);
        }
    }
}