using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
  class PermissionRepository : DataRepository<Permission, int>
  {
        public PermissionRepository(
            IServiceProvider serviceProvider,
            DataRepositoryContext context,
            IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        { }

        protected override ValueTask<EntityEntry<Permission>> AttachNewOrUpdateAsync(EntityEntry<Permission> entry, CancellationToken cancellationToken)
        {
            entry.Entity.Users = null;
            if (null != entry.Entity.ClientApplication)
            {
                if (entry.Entity.ClientApplication.HasValidId())
                {
                    entry.Entity.ClientApplicationId = entry.Entity.ClientApplication.Id;
                }
                entry.Entity.ClientApplication = null;
            }
            return base.AttachNewOrUpdateAsync(entry, cancellationToken);
        }
  }
}