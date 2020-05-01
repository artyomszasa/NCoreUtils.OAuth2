using System;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class ErrorResponse : IEquatable<ErrorResponse>, IErrorDescription
    {
        string? IErrorDescription.ErrorMessage => ErrorDescription;

        public string ErrorCode { get; }

        public string? ErrorDescription { get; }

        public ErrorResponse(string errorCode, string? errorDescription)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }

        public bool Equals(ErrorResponse other)
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
                throw new RemoteInvalidCredentialsException(endpoint, ErrorDescription ?? "Invalid credentials.");
            }
            throw new RemoteTokenServiceException(endpoint, ErrorCode, ErrorCode == TokenServiceErrorCodes.InternalError ? 500 : 400, ErrorDescription!);
        }
    }
}