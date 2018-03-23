[<RequireQualifiedAccess>]
module NCoreUtils.OAuth2.OAuth2ErrorMessages

[<CompiledName("InvalidAuthorizationCode")>]
let invalidAuthorizationCode = "Invalid authorization code."

[<CompiledName("InvalidRefreshToken")>]
let invalidRefreshToken = "Invalid refresh token.";

[<CompiledName("InvalidUser")>]
let invalidUser = "Invalid user.";

[<CompiledName("InvalidHost")>]
let invalidHost = "Invalid host.";

[<CompiledName("InvalidClientApplication")>]
let invalidClientApplication = "Invalid client application.";

[<CompiledName("InvalidUserCredentials")>]
let invalidUserCredentials = "Invalid user credentials.";

[<CompiledName("Missing")>]
let missing parameterName = sprintf "Missing %s parameter." parameterName

[<CompiledName("Invalid")>]
let invalid parameterName = sprintf "Invalid %s parameter." parameterName

[<CompiledName("UnsupportedGrantType")>]
let unsupportedGrantType grantType = sprintf "Unsupported %s parameter: %s." OAuth2Parameters.GrantType grantType

[<CompiledName("UnsufficientPermissionsToGrant")>]
let unsufficientPermissionsToGrant scope = sprintf "Requesting user has no permission to grant \"%s\" scope." scope
