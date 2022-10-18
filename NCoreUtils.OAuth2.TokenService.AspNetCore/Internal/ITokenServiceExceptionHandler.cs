using System;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.AspNetCore.Internal;

public interface ITokenServiceExceptionHandler
{
    bool ShouldPassException(HttpContext httpContext, Exception exn);
}