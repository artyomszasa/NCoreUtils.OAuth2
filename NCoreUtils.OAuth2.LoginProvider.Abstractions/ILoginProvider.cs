using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public interface ILoginProvider
    {
        ValueTask<LoginIdentity?> PasswordGrantAsync(
            string username,
            string password,
            ScopeCollection scopes,
            CancellationToken cancellationToken = default
        );

        ValueTask<LoginIdentity?> ExtensionGrantAsync(
            string type,
            string passcode,
            ScopeCollection scopes,
            CancellationToken cancellationToken = default
        );
    }
}