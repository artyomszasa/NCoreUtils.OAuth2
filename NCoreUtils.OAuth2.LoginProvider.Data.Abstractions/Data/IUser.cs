using System;
using System.Collections.Generic;

namespace NCoreUtils.OAuth2.Data
{
    public interface IUser<TId>
        where TId : IConvertible
    {
        TId Sub { get; }

        IEnumerable<string> GetAvailableScopes();
    }
}