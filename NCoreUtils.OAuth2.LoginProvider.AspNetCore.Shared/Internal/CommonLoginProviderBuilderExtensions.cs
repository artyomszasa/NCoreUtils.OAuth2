using System.Text.Json;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public static class CommonLoginProviderBuilderExtensions
    {
        public static void ApplyDefaultLoginProviderConfiguration(this ServiceDescriptorBuilder builder, string? prefix = default)
        {
            builder.Path = prefix ?? string.Empty;
            builder.NamingPolicy = NamingConvention.SnakeCase;
            builder.DefaultOutput = OutputType.Json(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { LoginIdentityConverter.Instance }
            });
            builder.DefaultInput = InputType.Json(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { ScopeCollectionConverter.Instance }
            });
        }
    }
}