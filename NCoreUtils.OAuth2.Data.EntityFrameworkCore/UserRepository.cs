using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    class UserRepository : DataRepository<User, int>
    {
        readonly IPasswordEncryption _passwordEncryption;

        readonly CurrentClientApplication _currentClientApplication;

        public override IQueryable<User> Items
            => base.Items
                .Include(e => e.Permissions)
                .Include(e => e.ClientApplication);

        public UserRepository(
            IServiceProvider serviceProvider,
            DataRepositoryContext context,
            IPasswordEncryption passwordEncryption,
            CurrentClientApplication currentClientApplication,
            IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        {
            _passwordEncryption = passwordEncryption ?? throw new ArgumentNullException(nameof(passwordEncryption));
            _currentClientApplication = currentClientApplication ?? throw new ArgumentNullException(nameof(currentClientApplication));
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
    }
}