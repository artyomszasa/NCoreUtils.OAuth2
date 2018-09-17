namespace NCoreUtils.OAuth2

open System.Runtime.InteropServices
open NCoreUtils.AspNetCore.Authentication.OpenId
open NCoreUtils.Authentication
open NCoreUtils.Interop
open FSharp.Control

type OpenIdWithScopesAuthenticationTicketEncoder<'TOpenIdClientConfiguration when 'TOpenIdClientConfiguration :> OpenIdClientConfiguration> =
  inherit OpenIdAuthenticationTicketEncoder<NCoreUtils.OAuth2.OpenIdUserInfo, 'TOpenIdClientConfiguration>

  new (configuration, loggerFactory, [<Optional;DefaultParameterValue(null:IUsernameFormatter)>] usernameFormatter) =
    { inherit OpenIdAuthenticationTicketEncoder<NCoreUtils.OAuth2.OpenIdUserInfo, 'TOpenIdClientConfiguration>(configuration, loggerFactory, usernameFormatter) }

  override this.GetClaimsAsync user =
    let source = base.GetClaimsAsync user |> ofAsyncEnumerable
    let roles =
      let roleType = this.RoleClaimType
      user.Scopes |> Seq.map (fun scope -> ClaimDescriptor(roleType, scope)) |> AsyncSeq.ofSeq
    AsyncSeq.append source roles |> toAsyncEnumerable
