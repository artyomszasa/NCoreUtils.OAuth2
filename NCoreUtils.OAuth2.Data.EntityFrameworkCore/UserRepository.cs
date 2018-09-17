using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    class UserRepository : DataRepository<User, int>
    {
        readonly ILogger _logger;

        readonly IPasswordEncryption _passwordEncryption;

        readonly CurrentClientApplication _currentClientApplication;

        public override IQueryable<User> Items
            => base.Items
                .Include(e => e.Permissions)
                .Include(e => e.Avatar)
                .Include(e => e.ClientApplication);

        public UserRepository(
            IServiceProvider serviceProvider,
            DataRepositoryContext context,
            IPasswordEncryption passwordEncryption,
            CurrentClientApplication currentClientApplication,
            ILogger<UserRepository> logger,
            IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        {
            _passwordEncryption = passwordEncryption ?? throw new ArgumentNullException(nameof(passwordEncryption));
            _currentClientApplication = currentClientApplication ?? throw new ArgumentNullException(nameof(currentClientApplication));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<EntityEntry<User>> AttachNewOrUpdateAsync(EntityEntry<User> entry, CancellationToken cancellationToken)
        {
            var password = entry.Entity.Password;

            if (null != entry.Entity.ClientApplication)
            {
                if (entry.Entity.ClientApplication.HasValidId())
                {
                    entry.Entity.ClientApplicationId = entry.Entity.ClientApplication.Id;
                }
                entry.Entity.ClientApplication = null;
            }

            if (null != entry.Entity.Avatar)
            {
                if (entry.Entity.Avatar.HasValidId())
                {
                    entry.Entity.AvatarId = entry.Entity.Avatar.Id;
                }
                entry.Entity.Avatar = null;
            }

            var e = await base.AttachNewOrUpdateAsync(entry, cancellationToken);
            if (e.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("No password specified.");
                }
                var encryptedPassword = _passwordEncryption.EncryptPassword(password);
                e.Entity.Password = encryptedPassword.PasswordHash;
                e.Entity.Salt = encryptedPassword.Salt;
                e.Entity.ClientApplicationId = _currentClientApplication.Id;
            }
            else
            {
                if (string.IsNullOrEmpty(password))
                {
                    var dbValues = await e.GetDatabaseValuesAsync(cancellationToken);
                    e.Entity.Password = dbValues.GetValue<string>(nameof(User.Password));
                    e.Entity.Salt = dbValues.GetValue<string>(nameof(User.Salt));
                    e.Property(x => x.Password).IsModified = false;
                    e.Property(x => x.Salt).IsModified = false;
                }
                else
                {
                    var encryptedPassword = _passwordEncryption.EncryptPassword(password);
                    e.Entity.Password = encryptedPassword.PasswordHash;
                    e.Entity.Salt = encryptedPassword.Salt;
                }
            }
            return e;
        }

        public override async Task<User> PersistAsync(User item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.PersistAsync(item, cancellationToken);
            var dbContext = _context.DbContext;
            // ensure permissions loaded
            {
                var entry = dbContext.Entry(result);
                var permissions = entry.Collection(e => e.Permissions);
                if (!permissions.IsLoaded)
                {
                    await permissions.Query().Include(p => p.Permission).LoadAsync(cancellationToken);
                }
            }
            // revoke all refresh tokens with out-of-date permissions
            var accessibleScopes = new HashSet<string>(result.Permissions.Select(up => up.Permission.Name), StringComparer.OrdinalIgnoreCase);
            var refreshTokens = await dbContext.Set<RefreshToken>().Where(token => token.UserId == result.Id).ToListAsync(cancellationToken);
            var revokeCount = 0;
            foreach (var refreshToken in refreshTokens)
            {
                if (refreshToken.Scopes.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(scope => !accessibleScopes.Contains(scope)))
                {
                    _logger.LogInformation("Revoking refresh token #{0} for user #{1} (reason: permissions changed).", refreshToken.Id, result.Id);
                    refreshToken.State = State.Deleted;
                }
            }
            if (0 < revokeCount)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            // return original result
            return result;
        }
    }
}