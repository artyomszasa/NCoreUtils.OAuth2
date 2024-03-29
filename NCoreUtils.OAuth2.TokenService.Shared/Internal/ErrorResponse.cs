using System;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal
{
    public class ErrorResponse : IEquatable<ErrorResponse>
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
        public string ErrorCode { get; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; }

        [JsonConstructor]
        public ErrorResponse(string errorCode, string? errorDescription)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }

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
}