using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public static class CommonTokenServiceBuilderExtensions
    {
        public static void ApplyDefaultTokenServiceConfiguration(this ServiceDescriptorBuilder builder, string? prefix = default)
        {
            builder.Path = prefix ?? string.Empty;
            builder.NamingPolicy = NamingConvention.SnakeCase;
            builder.DefaultOutput = OutputType.Json<TokenServiceJsonSerializerContext>();
            builder.DefaultError = ErrorType.Json<TokenServiceJsonSerializerContext>();
            builder.DefaultInput = InputType.UrlEncodedForm(default);
        }
    }
}