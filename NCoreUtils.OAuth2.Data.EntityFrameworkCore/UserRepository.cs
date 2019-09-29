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

        protected override async ValueTask<EntityEntry<User>> AttachNewOrUpdateAsync(EntityEntry<User> entry, CancellationToken cancellationToken)
        {
            var entity = entry.Entity;
            var password = entity.Password;

            if (null != entity.ClientApplication)
            {
                if (entity.ClientApplication.HasValidId())
                {
                    entity.ClientApplicationId = entity.ClientApplication.Id;
                }
                entity.ClientApplication = null;
            }

            if (null != entity.Avatar)
            {
                if (entity.Avatar.HasValidId())
                {
                    entity.AvatarId = entity.Avatar.Id;
                }
                entity.Avatar = null;
            }

            // Permissions
            var dbContext = EFCoreContext.DbContext;
            if (entity.HasValidId())
            {
                var fixedPermissions = new HashSet<UserPermission>();
                var toKeep = new HashSet<int>();
                if (null != entity.Permissions)
                {
                    foreach (var rel in entity.Permissions)
                    {
                        var dbRel = await dbContext.Set<UserPermission>().Where(up => up.UserId == entity.Id && up.PermissionId == rel.PermissionId).FirstOrDefaultAsync(cancellationToken);
                        if (null == dbRel)
                        {
                            var dbRelEntry = await dbContext.AddAsync(new UserPermission
                            {
                                PermissionId = rel.PermissionId,
                                UserId = entity.Id
                            });
                            dbRel = dbRelEntry.Entity;
                        }
                        fixedPermissions.Add(dbRel);
                        toKeep.Add(rel.PermissionId);
                    }
                }
                entity.Permissions = fixedPermissions;
                var toRemove = await dbContext.Set<UserPermission>().Where(up => up.UserId == entity.Id && !toKeep.Contains(up.PermissionId)).ToListAsync(cancellationToken);
                if (toRemove.Count > 0)
                {
                    dbContext.RemoveRange(toRemove);
                }
            }
            else
            {
                entity.ClientApplicationId = _currentClientApplication.Id;
                if (null != entity.Permissions)
                {
                    foreach (var rel in entity.Permissions)
                    {
                        rel.User = entity;
                    }
                }
                else
                {
                    entity.Permissions = new HashSet<UserPermission>();
                }
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
            var dbContext = EFCoreContext.DbContext;
            // ensure permissions loaded
            {
                var entry = dbContext.Entry(result);
                var permissions = entry.Collection(e => e.Permissions);
                if (!permissions.IsLoaded)
                {
                    await permissions.Query().Include(p => p.Permission).LoadAsync(cancellationToken);
                }
                foreach (var up in permissions.CurrentValue)
                {
                    var upEntry = dbContext.Entry(up);
                    var permission = upEntry.Reference(e => e.Permission);
                    if (!permission.IsLoaded)
                    {
                        await permission.LoadAsync(cancellationToken);
                    }
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