using System.Linq;
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
            ScopeCollection @null = new ScopeCollection(default);
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
        }

        [Fact]
        public void EmptynessTest()
        {
            ScopeCollection def = default;
            ScopeCollection @null = new ScopeCollection(default);
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
            ScopeCollection @null = new ScopeCollection(default);
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
    }
    #pragma warning restore xUnit2013
}