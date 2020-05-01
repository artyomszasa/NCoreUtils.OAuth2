using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public class RepositoryLoginProvider<TUser, TId> : ILoginProvider
        where TUser : ILocalUser<TId>
        where TId : IConvertible
    {
        private static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        private readonly ILoginProviderConfiguration _configuration;

        private readonly IDataRepository<TUser> _userRepository;

        public RepositoryLoginProvider(ILoginProviderConfiguration configuration, IDataRepository<TUser> userRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected virtual Expression<Func<TUser, bool>> CreateUsernamePredicate(string username)
        {
            if (_configuration.UseEmailAsUsername)
            {
                return e => e.Email == username;
            }
            return e => e.Username == username;
        }

        public ValueTask<LoginIdentity?> ExtensionGrantAsync(string type, string passcode, ScopeCollection scopes, CancellationToken cancellationToken = default)
            => default;

        public async ValueTask<LoginIdentity?> PasswordGrantAsync(string username, string password, ScopeCollection scopes, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.Items.FirstOrDefaultAsync(CreateUsernamePredicate(username), cancellationToken);
            if (user is null)
            {
                return null;
            }
            var sha512 = Sha512Helper.Rent();
            try
            {
                if (PasswordHelpers.ComputeHash(password, user.Salt) != user.Password)
                {
                    return null;
                }
                return new LoginIdentity(
                    user.Sub.ToString(CultureInfo.InvariantCulture),
                    _configuration.Issuer,
                    user.Username,
                    user.Email,
                    new ScopeCollection(user.GetAvailableScopes().Intersect(scopes))
                );
            }
            finally
            {
                Sha512Helper.Return(sha512);
            }
        }
    }
}