using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NCoreUtils.AspNetCore.Internal;

namespace NCoreUtils.AspNetCore;

public static class EndpointRouteBuilderTokenServiceExtensions
{
    public static IEndpointConventionBuilder MapTokenService(this IEndpointRouteBuilder endpoints, string? path = default)
        => endpoints.MapTokenServiceWrapper(path);
}