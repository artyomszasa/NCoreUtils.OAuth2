using System.Runtime.CompilerServices;

namespace NCoreUtils.OAuth2.Logging;

public class PasswordGrantNoUserEntry(bool useEmailAsUsername, string username)
{
    private string? _cachedMessage;

    public bool UseEmailAsUsername { get; } = useEmailAsUsername;

    public string Username { get; } = username ?? string.Empty;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string StringifyNoAlloc()
    {
        Span<char> buffer = stackalloc char[4 * 1024];
        var builder = new SpanBuilder(buffer);
        builder.Append(UseEmailAsUsername ? "No user found with email = " : "No user found with username = ");
        builder.Append(Username);
        builder.Append(" requested by password grant operation.");
        return builder.ToString();
    }

    public override string ToString()
    {
        if (_cachedMessage is null)
        {
            if (Username.Length < 512)
            {
                _cachedMessage = StringifyNoAlloc();
            }
            else
            {
                _cachedMessage = UseEmailAsUsername
                    ? $"No user found with email = {Username} requested by password grant operation."
                    : $"No user found with username = {Username} requested by password grant operation.";
            }
        }
        return _cachedMessage;
    }
}