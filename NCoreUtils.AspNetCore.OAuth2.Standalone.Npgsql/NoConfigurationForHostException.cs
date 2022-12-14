using System;

namespace NCoreUtils.AspNetCore.OAuth2;

internal class NoConfigurationForHostException : InvalidOperationException
{
    public NoConfigurationForHostException(string message) : base(message) { }
}