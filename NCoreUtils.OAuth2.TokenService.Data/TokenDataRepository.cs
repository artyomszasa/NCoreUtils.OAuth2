using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public class TokenDataRepository : ITokenRepository
    {
        public ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}