using System;

namespace NCoreUtils.OAuth2.Logging;

public static class L
{
    // TODO: use code generators
    public static readonly Func<object, Exception?, string> Fmt = (o, _) => o.ToString() ?? string.Empty;
}