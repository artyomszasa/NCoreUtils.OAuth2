using System;
using System.Linq;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    public class LoginIdentityTests
    {
        [Fact]
        public void InvalidArgs()
        {
            var exn = Assert.Throws<ArgumentException>(() => new LoginIdentity(default!, "issuer", "name", "email", default));
            Assert.Equal("sub", exn.ParamName);
            exn = Assert.Throws<ArgumentException>(() => new LoginIdentity("sub", default!, "name", "email", default));
            Assert.Equal("issuer", exn.ParamName);
            exn = Assert.Throws<ArgumentException>(() => new LoginIdentity("sub", "issuer", default!, "email", default));
            Assert.Equal("name", exn.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(2 * 1024)]
        [InlineData(8 * 1024)]
        public void StringifyNoEmail(int count)
        {
            var scopes = count == 0 ? default : new ScopeCollection(Enumerable.Range(0, count).Select(e => $"scope-{e}"));
            var identity = new LoginIdentity("sub", "issuer", "name", default, scopes);
            var expected = $"sub#name@issuer[{scopes}]";
            Assert.Equal(expected, identity.ToString());
            var buffer = new char[identity.GetEmplaceBufferSize()];
            Assert.Equal(buffer.Length, identity.Emplace(buffer.AsSpan()));
            Assert.Equal(expected, new string(buffer.AsSpan()));
            Assert.Throws<InsufficientBufferSizeException>(() => identity.Emplace(buffer.AsSpan().Slice(0, buffer.Length - 1)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(2 * 1024)]
        [InlineData(8 * 1024)]
        public void StringifyWithEmail(int count)
        {
            var scopes = count == 0 ? default : new ScopeCollection(Enumerable.Range(0, count).Select(e => $"scope-{e}"));
            var identity = new LoginIdentity("sub", "issuer", "name", "email", scopes);
            var expected = $"sub#name<email>@issuer[{scopes}]";
            Assert.Equal(expected, identity.ToString());
            var buffer = new char[identity.GetEmplaceBufferSize()];
            Assert.Equal(buffer.Length, identity.Emplace(buffer.AsSpan()));
            Assert.Equal(expected, new string(buffer.AsSpan()));
            Assert.Throws<InsufficientBufferSizeException>(() => identity.Emplace(buffer.AsSpan().Slice(0, buffer.Length - 1)));
        }

        [Fact]
        public void Equality()
        {
            var identity0 = new LoginIdentity("sub", "issuer", "name", "email", default);
            var identity1 = new LoginIdentity("sub", "issuer", "name", "email", default);
            var identity2 = new LoginIdentity("sub", "issuer", "name", "email", new ScopeCollection("a"));
            var identity3 = new LoginIdentity("sub1", "issuer", "name", "email", default);
            var identity4 = new LoginIdentity("sub", "issuer1", "name", "email", default);
            var identity5 = new LoginIdentity("sub", "issuer", "name1", "email", default);
            var identity6 = new LoginIdentity("sub", "issuer", "name", "email1", default);
            Assert.Equal(identity0.GetHashCode(), identity1.GetHashCode());
            Assert.Equal(identity0, identity1);
            Assert.NotEqual(identity0, default!);
            Assert.NotEqual(identity0, identity2);
            Assert.NotEqual(identity0, identity3);
            Assert.NotEqual(identity0, identity4);
            Assert.NotEqual(identity0, identity5);
            Assert.NotEqual(identity0, identity6);
            Assert.True(((object)identity0).Equals(identity1));
            Assert.False(((object)identity0).Equals(2));
        }
    }
}