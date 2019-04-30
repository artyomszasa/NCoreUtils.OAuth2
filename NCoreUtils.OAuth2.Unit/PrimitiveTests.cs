using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;
using Newtonsoft.Json;
using Xunit;

namespace NCoreUtils.OAuth2
{
    public class PrimitiveTests
    {
        static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        [Fact]
        public void TokenDeserializationFailures()
        {
            var empty = new byte[0];
            var invalid = new byte[10];
            Assert.Throws<FormatException>(() =>
            {
                using (var buffer = new MemoryStream(empty))
                using (var reader = new BinaryReader(buffer, _utf8))
                {
                    TokenModule.ReadFrom(reader);
                }
            });
            Assert.Throws<FormatException>(() =>
            {
                using (var buffer = new MemoryStream(invalid))
                using (var reader = new BinaryReader(buffer, _utf8))
                {
                    TokenModule.ReadFrom(reader);
                }
            });
        }

        [Fact]
        public void OAuth2ExceptionTests()
        {
            var exn = new OAuth2Exception(OAuth2Error.InvalidRequest, null);
            Assert.NotNull(exn.Message);

            var serializer = new BinaryFormatter();
            byte[] data;
            using (var buffer = new MemoryStream())
            {
                serializer.Serialize(buffer, exn);
                data = buffer.ToArray();
            }
            using (var buffer = new MemoryStream(data))
            {
                var exn0 = (OAuth2Exception) serializer.Deserialize(buffer);
                Assert.Equal(exn.Error, exn0.Error);
                Assert.Equal(exn.Message, exn0.Message);
            }

            var edata = OAuth2ExceptionExt.AsOAuth2Exception(exn);
            Assert.NotNull(edata);
            Assert.Equal(exn.Error, edata.Value.Item1);
            Assert.Equal(exn.ErrorDescription, edata.Value.Item2);
            Assert.Null(OAuth2ExceptionExt.AsOAuth2Exception(new InvalidOperationException()));
        }

        [Fact]
        public void TokenEquality()
        {
            var id = "xxxx";
            var dt = DateTimeOffset.Now;
            var t0 = Token.Create (id, dt, dt, new string[0]);
            var t1 = Token.Create (id, dt, dt, new string[0]);
            var t2 = Token.Create ("yyyy", dt, dt, new string[0]);
            var t3 = Token.Create (id, dt.AddDays(1), dt, new string[0]);
            var t4 = Token.Create (id, dt, dt.AddDays(1), new string[0]);
            var t5 = Token.Create (id, dt, dt, new [] { "xyz" });
            Assert.Equal(t0, t1);
            Assert.Equal(t0.GetHashCode(), t1.GetHashCode());
            Assert.Equal(t5.GetHashCode(), t5.GetHashCode());
            Assert.True(((object)t0).Equals(t1));
            Assert.True(t0.Equals(t1));
            Assert.NotEqual((object)2, (object)t0);
            Assert.NotEqual((object)t0, (object)2);
            Assert.False(Token.Eq(null, t0));
            Assert.False(Token.Eq(t0, null));
            Neq(t2);
            Neq(t3);
            Neq(t4);
            Neq(t5);

            void Neq(Token t)
            {
                Assert.NotEqual(t0, t);
                Assert.False(((object)t0).Equals(t));
                Assert.False(t0.Equals(t));
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        [Fact]
        public void EntityInterfaces()
        {
            var domain = (IHasId<int>)new Domain { Id = 1 };
            var permission = (IHasId<int>)new Permission { Id = 1 };
            var code = (IHasId<Guid>)new AuthorizationCode { Id = Guid.NewGuid() };
            Check(1, domain);
            Check(1, permission);
            Check(code.Id, code);

            void Check<T>(T expected, IHasId<T> obj) => Assert.Equal(expected, obj.Id);
        }

        [Fact]
        public void OAuth2ErrorConverterTests()
        {
            var converter = new OAuth2ErrorConverter();
            Assert.True(converter.CanConvert(typeof(OAuth2Error)));
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var writer = new StringWriter())
                using (var jwriter = new JsonTextWriter(writer))
                {
                    converter.WriteJson(jwriter, 'c', JsonSerializer.CreateDefault());
                }
            });
            using (var writer = new StringWriter())
            {
                using (var jwriter = new JsonTextWriter(writer))
                {
                    converter.WriteJson(jwriter, null, JsonSerializer.CreateDefault());
                }
                Assert.Equal("null", writer.ToString());
            }
            using (var reader = new StringReader($"{(int)OAuth2Error.AccessDenied}"))
            using (var jreader = new JsonTextReader(reader))
            {
                jreader.Read();
                Assert.Equal(OAuth2Error.AccessDenied, (OAuth2Error)converter.ReadJson(jreader, typeof(OAuth2Error), null, JsonSerializer.CreateDefault()));
            }
            using (var reader = new StringReader("{\"x\":2}"))
            using (var jreader = new JsonTextReader(reader))
            {
                jreader.Read();
                Assert.Throws<JsonSerializationException>(() => converter.ReadJson(jreader, typeof(OAuth2Error), null, JsonSerializer.CreateDefault()));
            }
        }
    }
}