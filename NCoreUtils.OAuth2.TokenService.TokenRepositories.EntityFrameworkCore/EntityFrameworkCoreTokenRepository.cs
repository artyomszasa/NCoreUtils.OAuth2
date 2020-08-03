using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public class EntityFrameworkCoreTokenRepository : ITokenRepository
    {
        protected IDataRepository<RefreshToken> Repository { get; }

        public EntityFrameworkCoreTokenRepository(IDataRepository<RefreshToken> repository)
            => Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            var issuedAt = token.IssuedAt.UtcTicks;
            var candidates = await Repository.Items.Where(e => e.Sub == token.Sub && e.IssuedAt == issuedAt).ToListAsync(cancellationToken);
            return candidates.Any(e => e.Scopes == string.Join(" ", token.Scopes));
        }

        public ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
            => new ValueTask(Repository.PersistAsync(new RefreshToken
            {
                Sub = token.Sub,
                Issuer = token.Issuer,
                Email = token.Email,
                Username = token.Username,
                Scopes = string.Join(" ", token.Scopes),
                IssuedAt = token.IssuedAt.UtcTicks,
                ExpiresAt = token.ExpiresAt.UtcTicks
            }, cancellationToken));
    }
}