using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class IntrospectionInputSerializer : InputSerializer
    {
        public static IntrospectionInputSerializer Instance { get; } = new IntrospectionInputSerializer();

        public override HttpRequestMessage CreateRequest(string endpoint, IReadOnlyList<Arg> arguments)
        {
            var args = new Dictionary<string, string>();
            foreach (var arg in arguments.Where(a => a.Name != "bearer_token"))
            {
                var value = arg.Serialize(default);
                if (null != value)
                {
                    args[arg.Name] = value;
                }
            }
            var content = new FormUrlEncodedContent(args);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var bearerToken = (arguments.FirstOrDefault(a => a.Name == "bearer_token") as Arg<string>)?.Value;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            }
            return request;
        }
    }
}