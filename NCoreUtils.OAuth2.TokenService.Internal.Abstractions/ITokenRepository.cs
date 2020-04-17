using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public interface ITokenRepository
    {
        ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default);

        ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default);
    }
}