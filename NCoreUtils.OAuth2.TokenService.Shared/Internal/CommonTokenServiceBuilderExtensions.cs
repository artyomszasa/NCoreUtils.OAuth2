using System.Text.Json;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public static class CommonTokenServiceBuilderExtensions
    {
        public static void ApplyDefaultTokenServiceConfiguration(this ServiceDescriptorBuilder builder, string? prefix = default)
        {
            builder.Path = prefix ?? string.Empty;
            builder.NamingPolicy = NamingPolicy.SnakeCase;
            builder.DefaultOutput = OutputType.Json(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { AccessTokenResponseConverter.Instance, IntrospectionResponseConverter.Instance }
            });
            builder.DefaultInput = InputType.UrlEncodedForm(default);
        }
    }
}