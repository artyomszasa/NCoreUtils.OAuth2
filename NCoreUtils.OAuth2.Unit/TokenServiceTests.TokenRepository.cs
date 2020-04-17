using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Unit
{
    public partial class TokenServiceTests
    {
        private sealed class TokenRepository : ITokenRepository
        {
            private HashSet<Token> _tokens = new HashSet<Token>();

            public ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
                => new ValueTask<bool>(_tokens.Contains(token));

            public ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
            {
                _tokens.Add(token);
                return default;
            }
        }
    }
}