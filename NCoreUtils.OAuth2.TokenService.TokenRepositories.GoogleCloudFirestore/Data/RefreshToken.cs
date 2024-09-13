using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data;

public class RefreshToken(
    string id,
    string sub,
    string issuer,
    string? email,
    string username,
    string scopes,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt) : IHasId<string>
{
    public string Id { get; } = id;

    public string Sub { get; } = sub;

    public string Issuer { get; } = issuer;

    public string? Email { get; } = email;

    public string Username { get; } = username;

    public string Scopes { get; } = scopes;

    public DateTimeOffset IssuedAt { get; } = issuedAt;

    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}