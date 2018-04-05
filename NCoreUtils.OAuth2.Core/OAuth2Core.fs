namespace NCoreUtils.OAuth2

open System
open System.Security.Claims
open System.Text.RegularExpressions
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open NCoreUtils
open NCoreUtils.Authentication
open NCoreUtils.Data
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data
open System.Runtime.CompilerServices

type OAuth2Core (loginAuthenticator : LoginAuthenticator,
                  refreshTokenRepository : IDataRepository<RefreshToken>,
                  authorizationCodeRepository : IDataRepository<AuthorizationCode, Guid>,
                  configurationOptions : IOptions<OAuth2Configuration>,
                  currentClientApplication : CurrentClientApplication,
                  logger : ILogger<OAuth2Core>) =

  static let separator = new Regex (@"\s*,\s*", RegexOptions.Compiled ||| RegexOptions.CultureInvariant);

  static let toLower (input : string) = input.ToLowerInvariant ()

  static let noAuthorizationCode () =
    OAuth2Exception (OAuth2Error.InvalidGrant, OAuth2ErrorMessages.invalidAuthorizationCode) |> raise : AuthorizationCode

  static let invalidUser () =
    OAuth2Exception (OAuth2Error.ServerError, OAuth2ErrorMessages.invalidUser) |> raise

  static let invalidRefreshToken () =
    OAuth2Exception (OAuth2Error.InvalidGrant, OAuth2ErrorMessages.invalidRefreshToken) |> raise

  static let vfst (struct (x, _)) = x

  static let vsnd (struct (_, x)) = x

  let configuration = configurationOptions.Value

  let createTokensAsync userId (grantedScopes : string[]) = async {
    let issuedAt = DateTimeOffset.Now
    let grantedScopes' = Seq.map CaseInsensitive grantedScopes |> Set.ofSeq
    // create access token
    let accessToken =
      { Id        = userId.ToString ()
        IssuedAt  = issuedAt
        ExpiresAt = issuedAt + configuration.AccessTokenExpiry
        Scopes    = grantedScopes' }
    // create refreshToken
    let refreshToken =
      { Id        = userId.ToString ()
        IssuedAt  = issuedAt
        ExpiresAt = issuedAt + configuration.RefreshTokenExpiry
        Scopes    = grantedScopes' }
    // persist refresh token
    Array.Sort (grantedScopes, StringComparer.OrdinalIgnoreCase); // scopes stored as sorted array
    do!
      refreshTokenRepository.AsyncPersist
        { Id        = Unchecked.defaultof<_>
          State     = State.Public
          UserId    = userId
          User      = Unchecked.defaultof<_>
          IssuedAt  = refreshToken.IssuedAt.UtcTicks
          ExpiresAt = refreshToken.ExpiresAt.UtcTicks
          Scopes    = String.concat "," grantedScopes
          LastUsed  = Nullable.empty }
      |> Async.Ignore
    return (accessToken, Some refreshToken) }

  let authenticateUser (username : string) password = async {
    logger.LogTrace("Authenticating \"{0}\" using password authentication.", username);
    let! claims = Async.Adapt (fun cancellationToken -> loginAuthenticator.AuthenticateAsync ("password", sprintf "%s:%s" username password, cancellationToken));
    return
      match claims with
      | null ->
        logger.LogDebug("Failed to authenticate \"{0}\" using password authentication.", username)
        OAuth2Exception (OAuth2Error.AccessDenied, OAuth2ErrorMessages.invalidUserCredentials) |> raise
      | _ ->
        logger.LogDebug("Successfully authenticated \"{0}\" using password authentication.", username)
        claims }

  let validateUserAndScopes (claims : ClaimCollection) (clientApplicationId : int) (scopes : string) =
    // obtain user id
    let id =
      claims
      |>  Claims.tryClaimValue ClaimTypes.Sid
      >>= tryInt
      |>  Option.defaultWith invalidUser
    // validate that authenticated user belongs to the actual client application
    let isClientApplicationIdValid =
      claims
      |> Claims.tryClaimValue Claims.ClientApplicationId
      >>= tryInt
      |> Option.exists ((=) clientApplicationId)
    if not isClientApplicationIdValid then
      OAuth2Exception (OAuth2Error.AccessDenied, OAuth2ErrorMessages.invalidUserCredentials) |> raise
    // validate that user has sufficient permissions to grant requested scopes
    let availableScopes =
      claims
      |> Seq.filter (fun claim -> claim.Type = claims.RoleClaimType)
      |> Seq.map    (fun claim -> toLower claim.Value)
      |> Set.ofSeq
    let grantedScopes =
      match String.IsNullOrWhiteSpace scopes with
      | true ->
        logger.LogTrace ("No requested scopes, granting all available scopes for #{0}: {1}.", id, String.concat "," availableScopes);
        Seq.toArray availableScopes
      | _ ->
        logger.LogTrace ("Validating requested scopes for #{0}: {1}.", id, scopes)
        let requestedScopes = separator.Split scopes |> Array.map toLower
        // raise error if some requested scope is not present in available scopes
        requestedScopes
          |> Array.tryFind (fun scope -> not (Set.contains scope availableScopes))
          |> Option.iter   (fun scope -> OAuth2Exception (OAuth2Error.InvalidScope, OAuth2ErrorMessages.unsufficientPermissionsToGrant scope) |> raise)
        logger.LogTrace("Successfully validated requested scopes for #{0}: {1}.", id, scopes)
        requestedScopes
    id, grantedScopes

  let authenticateAndValidate clientApplicationId username password scopes = async {
    currentClientApplication.Id <- clientApplicationId
    let! claims = authenticateUser username password
    return validateUserAndScopes claims clientApplicationId scopes }

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member __.CreateAuthorizationCodeByPasswordAsync (clientApplicationId, redirectUri, username, password, scopes) = async {
    let! (userId, grantedScopes) = authenticateAndValidate clientApplicationId username password scopes
    let! authCode =
      let now = DateTimeOffset.Now
      Array.Sort (grantedScopes, StringComparer.OrdinalIgnoreCase) // scopes stored as sorted array
      let authCode =
        { Id          = Guid.NewGuid()
          UserId      = userId
          User        = Unchecked.defaultof<_>
          IssuedAt    = now.UtcTicks
          ExpiresAt   = (now + configuration.AuthorizationCodeExpiry).UtcTicks
          RedirectUri = redirectUri
          Scopes      = String.concat "," grantedScopes }
      authorizationCodeRepository.AsyncPersist authCode
    return authCode.Id }

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member __.AuthenticateByPasswordAsync (clientApplicationId, username, password, scopes) =
    authenticateAndValidate clientApplicationId username password scopes
    >>= (<||) createTokensAsync

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member __.AuthenticateByCodeAsync (clientApplicationId, redirectUri, authorizationCodeGuid) =
    authorizationCodeRepository.Context.AsyncTransacted (
      System.Data.IsolationLevel.ReadCommitted,
      fun () ->
          authorizationCodeRepository.Items
          |>  Q.filter (fun code -> code.Id = authorizationCodeGuid && code.RedirectUri = redirectUri && code.User.ClientApplicationId = clientApplicationId)
          |>  Q.asyncTryFirst
          >>| Option.defaultWith noAuthorizationCode
          >>+ (fun authCode -> createTokensAsync authCode.UserId (authCode.Scopes.Split ','))
          >>* (fst >> authorizationCodeRepository.AsyncRemove)
          >>| snd)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member __.RefreshTokenAsync (refreshToken : Token) =
    refreshTokenRepository.Context.AsyncTransacted (
      System.Data.IsolationLevel.ReadCommitted,
      fun () ->
        let grantedScopes = refreshToken.Scopes |> Seq.map (fun scope -> scope.ToLowerString ()) |> Seq.toArray
        Array.Sort (grantedScopes, StringComparer.OrdinalIgnoreCase)
        let scopes            = String.concat "," grantedScopes
        let userId            = tryInt refreshToken.Id |> Option.getOrDef -1
        let issuedAtUtcTicks  = refreshToken.IssuedAt.UtcTicks
        let expiresAtUtcTicks = refreshToken.ExpiresAt.UtcTicks
        refreshTokenRepository.Items
        |> Q.filter (fun rt -> rt.State = State.Public && rt.UserId = userId && rt.IssuedAt = issuedAtUtcTicks && rt.ExpiresAt = expiresAtUtcTicks && rt.Scopes = scopes)
        |> Q.asyncTryFirst
        >>| Option.defaultWith invalidRefreshToken
        >>+ (fun rtoken ->
              let now = DateTimeOffset.Now
              rtoken.LastUsed <- Nullable.mk now.UtcTicks
              // create access token
              async.Return
                { Id        = userId.ToString ()
                  IssuedAt  = now
                  ExpiresAt = now + configuration.AccessTokenExpiry
                  Scopes    = refreshToken.Scopes })
        >>* (fst >> refreshTokenRepository.AsyncPersist >> Async.Ignore)
        >>| snd)

  interface IOAuth2Core with
    member __.Logger = logger :> _
    member this.CreateAuthorizationCodeByPasswordAsync (clientApplicationId, redirectUri, username, password, scopes) =
      this.CreateAuthorizationCodeByPasswordAsync (clientApplicationId, redirectUri, username, password, scopes)
    member this.AuthenticateByPasswordAsync (clientApplicationId, username, password, scopes) =
      this.AuthenticateByPasswordAsync (clientApplicationId, username, password, scopes)
    member this.AuthenticateByCodeAsync (clientApplicationId, redirectUri, authorizationCodeGuid) =
      this.AuthenticateByCodeAsync (clientApplicationId, redirectUri, authorizationCodeGuid)
    member this.RefreshTokenAsync refreshToken =
      this.RefreshTokenAsync refreshToken