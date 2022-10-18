using System;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.AspNetCore.Internal;

public interface ILoginProviderExceptionHandler
{
    bool ShouldPassException(HttpContext httpContext, Exception exn);
}