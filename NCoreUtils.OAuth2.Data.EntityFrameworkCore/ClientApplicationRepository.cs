using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    class ClientApplicationRepository : DataRepository<ClientApplication, int>
    {
        static async Task FixDomains(DbContext dbContext, ClientApplication app, CancellationToken cancellationToken)
        {
            if (app.HasValidId())
            {
                var fixedDomains = new HashSet<Domain>();
                var domainIdsToKeep = new HashSet<int>();
                if (null != app.Domains)
                {
                    foreach (var domain in app.Domains)
                    {
                        Domain dbDomain;
                        if (domain.HasValidId())
                        {
                            dbDomain = await dbContext.Set<Domain>()
                                .Where(e => e.Id == domain.Id)
                                .FirstOrDefaultAsync(cancellationToken);
                            if (null == dbDomain)
                            {
                                throw new InvalidOperationException($"Invalid domain id: {domain.Id}.");
                            }
                            dbDomain.DomainName = domain.DomainName;
                        }
                        else
                        {
                            var dbDomainEntry = await dbContext.AddAsync(new Domain
                            {
                                ClientApplicationId = app.Id,
                                DomainName = domain.DomainName
                            });
                            dbDomain = dbDomainEntry.Entity;
                        }
                        fixedDomains.Add(dbDomain);
                        domainIdsToKeep.Add(dbDomain.Id);
                    }
                }
                var toRemove = await dbContext.Set<Domain>()
                    .Where(e => e.ClientApplicationId == app.Id && !domainIdsToKeep.Contains(e.Id))
                    .ToListAsync(cancellationToken);
                if (0 != toRemove.Count)
                {
                    dbContext.RemoveRange(toRemove);
                }
                app.Domains = fixedDomains;
            }
            else
            {
                if (null != app.Domains)
                {
                    foreach (var domain in app.Domains)
                    {
                        domain.ClientApplication = app;
                    }
                }
            }
        }

        public ClientApplicationRepository(
            IServiceProvider serviceProvider,
            DataRepositoryContext context,
            IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        { }

        protected override async Task<EntityEntry<ClientApplication>> AttachNewOrUpdateAsync(
            EntityEntry<ClientApplication> entry,
            CancellationToken cancellationToken)
        {
            await FixDomains(EFCoreContext.DbContext, entry.Entity, cancellationToken);
            entry.Entity.Permissions = null;
            entry.Entity.Users = null;
            return await base.AttachNewOrUpdateAsync(entry, cancellationToken);
        }
    }
}