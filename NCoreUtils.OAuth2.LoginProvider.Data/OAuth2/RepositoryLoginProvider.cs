using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;
using NCoreUtils.OAuth2.Internal;
using NCoreUtils.OAuth2.Logging;

namespace NCoreUtils.OAuth2
{
    public class RepositoryLoginProvider<TUser, TId> : ILoginProvider
        where TUser : ILocalUser<TId>
        where TId : IConvertible
    {
        private static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        protected ILoginProviderConfiguration Configuration { get; }

        protected IDataRepository<TUser> UserRepository { get; }

        protected ILogger Logger { get; }

        public RepositoryLoginProvider(
            ILoginProviderConfiguration configuration,
            IDataRepository<TUser> userRepository,
            ILogger<ILoginProvider> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected virtual Expression<Func<TUser, bool>> CreateUsernamePredicate(string username)
        {
            if (Configuration.UseEmailAsUsername)
            {
                return e => e.Email == username;
            }
            return e => e.Username == username;
        }

        public ValueTask<LoginIdentity?> ExtensionGrantAsync(string type, string passcode, ScopeCollection scopes, CancellationToken cancellationToken = default)
            => default;

        public async ValueTask<LoginIdentity?> PasswordGrantAsync(string username, string password, ScopeCollection scopes, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.Items.FirstOrDefaultAsync(CreateUsernamePredicate(username), cancellationToken);
            if (user is null)
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.Log(LogLevel.Debug, default, new PasswordGrantNoUserEntry(Configuration.UseEmailAsUsername, username), default, L.Fmt);
                }
                return null;
            }
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.Log(LogLevel.Trace, default, new PaswordGrantUserFoundEntry<TId>(Configuration.UseEmailAsUsername, username, user), default, L.Fmt);
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
                    Configuration.Issuer,
                    user.Username,
                    user.Email,
                    new ScopeCollection(scopes.HasValue ? user.GetAvailableScopes().Intersect(scopes) : user.GetAvailableScopes())
                );
            }
            finally
            {
                Sha512Helper.Return(sha512);
            }
        }
    }
}