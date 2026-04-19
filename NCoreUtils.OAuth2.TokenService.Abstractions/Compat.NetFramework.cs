namespace NCoreUtils.OAuth2;

internal static class NetFrameworkCompatExtensions
{
    public static void Deconstruct(this KeyValuePair<string, string?> source, out string key, out string? value)
    {
        key = source.Key;
        value = source.Value;
    }
}