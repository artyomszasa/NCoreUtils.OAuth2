using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NCoreUtils.OAuth2.Internal;
using Xunit;

namespace NCoreUtils.OAuth2.Unit
{
    #pragma warning disable xUnit2013
    public class ScopeCollectionTests
    {
        [Fact]
        public void EqualityTest()
        {
            ScopeCollection def = default;
            ScopeCollection @null = new ScopeCollection(default(IEnumerable<string>));
            ScopeCollection empty = new ScopeCollection(Enumerable.Empty<string>());
            ScopeCollection nonEmpty0 = new ScopeCollection(new [] { "xxx" });
            ScopeCollection copy = nonEmpty0;
            ScopeCollection nonEmpty1 = new ScopeCollection(new [] { "xxx" });
            #pragma warning disable xUnit2000
            Assert.Equal(def, default);
            #pragma warning restore xUnit2000
            Assert.Equal(def, @null);
            Assert.Equal(def.GetHashCode(), @null.GetHashCode());
            Assert.Equal(nonEmpty0, copy);
            Assert.Equal(nonEmpty0, nonEmpty1);
            Assert.Equal(nonEmpty0.GetHashCode(), nonEmpty1.GetHashCode());
            #pragma warning disable xUnit2000
            Assert.NotEqual(empty, default);
            #pragma warning restore xUnit2000
            Assert.NotEqual(empty, def);
            Assert.NotEqual(empty, @null);
            Assert.NotEqual(empty, nonEmpty0);
            Assert.NotEqual(def, nonEmpty0);

            Assert.True(((object)def).Equals(@null));
            Assert.True(((object)nonEmpty0).Equals(nonEmpty1));
            Assert.False(((object)nonEmpty0).Equals(2));
        }

        [Fact]
        public void EmptynessTest()
        {
            ScopeCollection def = default;
            ScopeCollection @null = new ScopeCollection(default(IEnumerable<string>));
            ScopeCollection empty = new ScopeCollection(Enumerable.Empty<string>());
            ScopeCollection nonEmpty0 = new ScopeCollection(new [] { "xxx" });
            ScopeCollection copy = nonEmpty0;
            ScopeCollection nonEmpty1 = new ScopeCollection(new [] { "xxx" });
            Assert.True(def.IsEmpty);
            Assert.True(@null.IsEmpty);
            Assert.True(empty.IsEmpty);
            Assert.False(nonEmpty0.IsEmpty);
            Assert.False(copy.IsEmpty);
            Assert.False(nonEmpty1.IsEmpty);
        }

        [Fact]
        public void EnumerationTest()
        {
            ScopeCollection def = default;
            ScopeCollection @null = new ScopeCollection(default(IEnumerable<string>));
            ScopeCollection empty = new ScopeCollection(Enumerable.Empty<string>());
            ScopeCollection nonEmpty = new ScopeCollection(new [] { "xxx" });
            Assert.Equal(0, def.Count());
            Assert.Equal(0, @null.Count());
            Assert.Equal(0, empty.Count());
            Assert.Equal(1, nonEmpty.Count());
            Assert.True(nonEmpty.SequenceEqual(new [] { "xxx" }));
            var enumerator = ((System.Collections.IEnumerable)nonEmpty).GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal((object)"xxx", enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(2 * 1024)]
        [InlineData(8 * 1024)]
        public void StringificationTest(int count)
        {
            var scopes = count == 0 ? default : new ScopeCollection(Enumerable.Range(0, count).Select(e => $"scope-{e}"));
            Assert.Equal(string.Join(' ', scopes), scopes.ToString());
            var buffer = new char[scopes.ComputeRequiredBufferSize()];
            Assert.Equal(buffer.Length, scopes.Emplace(buffer.AsSpan()));
            Assert.Equal(string.Join(' ', scopes), new string(buffer.AsSpan()));
            if (buffer.Length > 0)
            {
                Assert.Throws<InsufficientBufferSizeException>(() => scopes.Emplace(buffer.AsSpan().Slice(0, buffer.Length - 1)));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(2 * 1024)]
        [InlineData(8 * 1024)]
        public void JsonReserializeTests(int count)
        {
            var scopes = count == 0 ? default : new ScopeCollection(Enumerable.Range(0, count).Select(e => $"scope-{e}"));
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { ScopeCollectionConverter.Instance }
            };
            Assert.Equal(scopes, JsonSerializer.Deserialize<ScopeCollection>(JsonSerializer.Serialize(scopes, options), options));
        }

        [Fact]
        public void JsonSerializationTests()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { ScopeCollectionConverter.Instance }
            };
            Assert.Equal(default(ScopeCollection), JsonSerializer.Deserialize<ScopeCollection>("null", options));
            Assert.Equal(new ScopeCollection("a", "b"), JsonSerializer.Deserialize<ScopeCollection>("[\"a\",\"b\"]", options));
            Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ScopeCollection>("{}", options));
            var exn = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ScopeCollection>("[\"a\", 1]", options));
            Assert.Equal("$", exn.Path);
            Assert.Equal(0, exn.LineNumber);
            Assert.Equal(7, exn.BytePositionInLine);
        }
    }
    #pragma warning restore xUnit2013
}