using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Unit
{
    public partial class TokenServiceTests
    {
        private sealed class User : LoginIdentity
        {
            public string Password { get; }

            public string Passcode { get; }

            public User(string sub, string password, string passcode, string issuer, string name, string? email, ScopeCollection scopes)
                : base(sub, issuer, name, email, scopes)
            {
                Password = password;
                Passcode = passcode;
            }
        }

        private sealed class LoginProvider : ILoginProvider
        {
            public IReadOnlyList<User> Users { get; }

            public LoginProvider(IReadOnlyList<User> users)
                => Users = users ?? throw new ArgumentNullException(nameof(users));


            public ValueTask<LoginIdentity?> ExtensionGrantAsync(string type, string passcode, ScopeCollection scopes, CancellationToken cancellationToken = default)
            {
                if (type != ExtensionGrantType)
                {
                    return default;
                }
                var user = Users.FirstOrDefault(e => e.Passcode == passcode);
                if (user is null)
                {
                    return default;
                }
                return new ValueTask<LoginIdentity?>(user);
            }

            public ValueTask<LoginIdentity?> PasswordGrantAsync(string username, string password, ScopeCollection scopes, CancellationToken cancellationToken = default)
            {
                var user = Users.FirstOrDefault(e => e.Email == username && e.Password == password);
                if (user is null)
                {
                    return default;
                }
                return new ValueTask<LoginIdentity?>(user);
            }
        }
    }
}