using System;

namespace NCoreUtils.OAuth2.Data;

public interface ILocalUser<TId> : IUser<TId>
    where TId : IConvertible
{
    string? Email { get; }

    string Username { get; }

    /// <summary>
    /// Password hash (base64 encoded string).
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Random generated salt to use when computing password hash.
    /// </summary>
    string Salt { get; }
}