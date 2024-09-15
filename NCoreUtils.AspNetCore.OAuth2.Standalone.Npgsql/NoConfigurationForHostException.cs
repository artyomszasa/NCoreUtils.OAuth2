using System;

namespace NCoreUtils.AspNetCore.OAuth2;

internal class NoConfigurationForHostException(string message)
    : InvalidOperationException(message)
{ }
