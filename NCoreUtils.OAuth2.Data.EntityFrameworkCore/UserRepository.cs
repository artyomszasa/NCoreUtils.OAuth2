using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    class UserRepository : DataRepository<User, int>
    {
        public override IQueryable<User> Items
            => base.Items
                .Include(e => e.Permissions)
                .Include(e => e.ClientApplication);

        public UserRepository(IServiceProvider serviceProvider, DataRepositoryContext context, IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        { }
    }
}