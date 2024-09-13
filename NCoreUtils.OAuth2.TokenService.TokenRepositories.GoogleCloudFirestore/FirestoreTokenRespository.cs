#if NET7_0
using System.Diagnostics.CodeAnalysis;
#endif
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2;

public class FirestoreTokenRepository(IDataRepository<RefreshToken> repository) : ITokenRepository
{
    protected IDataRepository<RefreshToken> Repository { get; } = repository ?? throw new ArgumentNullException(nameof(repository));

#if NET7_0
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("AOT", "IL3050")]
#endif
    public async ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
    {
        var candidates = await Repository.Items.Where(e => e.Sub == token.Sub && e.IssuedAt == token.IssuedAt).ToListAsync(cancellationToken);
        return candidates.Any(e => e.Scopes == string.Join(" ", token.Scopes));
    }

    public ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
        => new(Repository.PersistAsync(new RefreshToken(
            default!,
            token.Sub,
            token.Issuer,
            token.Email,
            token.Username,
            string.Join(" ", token.Scopes),
            token.IssuedAt,
            token.ExpiresAt
        ), cancellationToken));
}