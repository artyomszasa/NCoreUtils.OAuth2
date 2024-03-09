namespace NCoreUtils.OAuth2;

public static class TokenServiceErrorCodes
{
    public const string AccessDenied = "access_denied";

    public const string InvalidCredentials = "invalid_credentials";

    public const string InternalError = "server_error";

    public const string MissingGrantType = "missing_grant_type";

    public const string MissingPassword = "missing_password";

    public const string MissingUsername = "missing_username";

    public const string MissingRefreshToken = "missing_refresh_token";

    public const string MissingPasscode = "missing_passcode";

    public const string MissingToken = "missing_token";
}