using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2.Internal
{
    internal static class FormCollectionExtensions
    {
        private static Regex _regexWhitespace = new Regex("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static bool TryGetFirstValue(this IFormCollection form, string key, [NotNullWhen(true)] out string? value)
        {
            if (form.TryGetValue(key, out var values) && values.Count > 0)
            {
                value = values[0];
                return true;
            }
            value = default;
            return false;
        }
    }
}