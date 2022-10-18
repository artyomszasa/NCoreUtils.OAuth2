using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore.Internal;
using NCoreUtils.OAuth2.LoginProvider;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore;

[ProtoService(typeof(LoginProviderInfo), typeof(LoginProviderSerializerContext))]
public class LoginProviderService { }

public partial class ProtoLoginProviderServiceImplementation
{
    private static bool ShouldPassException(HttpContext context, Exception exn)
    {
        if (!context.RequestAborted.IsCancellationRequested)
        {
            return false;
        }
        if (exn is OperationCanceledException)
        {
            return true;
        }
        foreach (var handler in context.RequestServices.GetServices<ILoginProviderExceptionHandler>())
        {
            if (handler.ShouldPassException(context, exn))
            {
                return true;
            }
        }
        return false;
    }
}