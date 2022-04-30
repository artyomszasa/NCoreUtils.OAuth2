using System.Diagnostics.CodeAnalysis;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public static class CommonLoginProviderBuilderExtensions
    {
#if NET6_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LoginProviderSerializationContext))]
#endif
        public static void ApplyDefaultLoginProviderConfiguration(this ServiceDescriptorBuilder builder, string? prefix = default)
        {
            builder.Path = prefix ?? string.Empty;
            builder.NamingPolicy = NamingConvention.SnakeCase;
            builder.DefaultOutput = OutputType.Json<LoginProviderSerializationContext>();
            builder.DefaultInput = InputType.Json<LoginProviderSerializationContext>();
        }
    }
}