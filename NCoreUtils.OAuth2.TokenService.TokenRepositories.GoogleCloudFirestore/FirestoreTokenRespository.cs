using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public class FirestoreTokenRepository : ITokenRepository
    {
        protected IDataRepository<RefreshToken> Repository { get; }

        public FirestoreTokenRepository(IDataRepository<RefreshToken> repository)
            => Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            var candidates = await Repository.Items.Where(e => e.Sub == token.Sub && e.IssuedAt == token.IssuedAt).ToListAsync(cancellationToken);
            return candidates.Any(e => e.Scopes == string.Join(" ", token.Scopes));
        }

        public ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
            => new ValueTask(Repository.PersistAsync(new RefreshToken(
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
}