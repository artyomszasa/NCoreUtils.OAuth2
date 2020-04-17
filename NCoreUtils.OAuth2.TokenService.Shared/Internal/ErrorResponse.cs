using System;

namespace NCoreUtils.OAuth2.Internal
{
    public class ErrorResponse : IEquatable<ErrorResponse>
    {
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
    }
}