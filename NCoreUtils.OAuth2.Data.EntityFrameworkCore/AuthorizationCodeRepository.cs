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
    class AuthorizationCodeRepository : DataRepository<AuthorizationCode, Guid>
    {
        public AuthorizationCodeRepository(
            IServiceProvider serviceProvider,
            DataRepositoryContext context,
            IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        { }

        protected override async Task<EntityEntry<AuthorizationCode>> AttachNewOrUpdateAsync(
            EntityEntry<AuthorizationCode> entry,
            CancellationToken cancellationToken)
        {
            var dbContext = EFCoreContext.DbContext;
            if (await dbContext.Set<AuthorizationCode>().AnyAsync(e => e.Id == entry.Entity.Id, cancellationToken))
            {
                // check whether another instance is already tracked
                var existentEntry = dbContext.ChangeTracker.Entries<AuthorizationCode>().FirstOrDefault(e => e.Entity.Id == entry.Entity.Id);
                if (null == existentEntry)
                {
                    var updatedEntry = dbContext.Update(entry.Entity);
                    await PrepareUpdatedEntityAsync(updatedEntry, cancellationToken).ConfigureAwait(false);
                    return updatedEntry;
                }
                existentEntry.CurrentValues.SetValues(entry.Entity);
                await PrepareUpdatedEntityAsync(existentEntry, cancellationToken).ConfigureAwait(false);
                return existentEntry;
            }
            var addedEntry = await dbContext.AddAsync(entry.Entity, cancellationToken).ConfigureAwait(false);
            await PrepareAddedEntityAsync(addedEntry, cancellationToken);
            return addedEntry;
        }
    }
}