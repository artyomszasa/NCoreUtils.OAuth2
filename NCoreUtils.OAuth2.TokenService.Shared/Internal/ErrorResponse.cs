using System;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[method: JsonConstructor]
public class ErrorResponse(string errorCode, string? errorDescription) : IEquatable<ErrorResponse>
{
    public static bool operator==(ErrorResponse? a, ErrorResponse? b)
    {
        if (a is null)
        {
            return b is null;
        }
        return a.Equals(b);
    }

    public static bool operator!=(ErrorResponse? a, ErrorResponse? b)
    {
        if (a is null)
        {
            return b is not null;
        }
        return !a.Equals(b);
    }

    [JsonPropertyName("error")]
    public string ErrorCode { get; } = errorCode;

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; } = errorDescription;

    public bool Equals(ErrorResponse? other)
        => other != null
            && ErrorCode == other.ErrorCode
            && ErrorDescription == other.ErrorDescription;

    public override bool Equals(object? obj)
        => obj is ErrorResponse other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(ErrorCode, ErrorDescription);

    public Exception ToException(string endpoint)
    {
        if (ErrorCode == TokenServiceErrorCodes.AccessDenied)
        {
            return new RemoteAccessDeniedException(endpoint, ErrorDescription ?? "Access denied.");
        }
        if (ErrorCode == TokenServiceErrorCodes.InvalidCredentials)
        {
            return new RemoteInvalidCredentialsException(endpoint, ErrorDescription ?? "Invalid credentials.");
        }
        return new RemoteTokenServiceException(endpoint, ErrorCode, ErrorCode == TokenServiceErrorCodes.InternalError ? 500 : 400, ErrorDescription!);
    }
}