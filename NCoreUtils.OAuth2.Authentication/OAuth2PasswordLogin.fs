namespace NCoreUtils.OAuth2

open System
open System.Linq
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open NCoreUtils.Authentication
open NCoreUtils.OAuth2.Data


type OAuth2PasswordLogin (serviceProvider : IServiceProvider,
                          userManager     : OAuth2UserManager,
                          logger          : ILogger<OAuth2PasswordLogin>) =
  inherit PasswordLogin<int> (userManager, logger :?> ILogger<PasswordLogin<int>>, serviceProvider.GetService<PasswordLoginOptions> (), serviceProvider.GetService<IUsernameFormatter> ())

  override __.GetClaimsAsync (user, forceName) =
    let baseClaims = base.GetClaimsAsync(user, forceName)
    match user with
    | :? User as user ->
      let claims =
        match obj.ReferenceEquals (user.ClientApplication, null) with
        | true ->
          [|  ClaimDescriptor (Claims.ClientApplicationId, user.ClientApplictionId.ToString ()) |]
        | _ ->
          [|  ClaimDescriptor (Claims.ClientApplicationId,   user.ClientApplication.Id.ToString ())
              ClaimDescriptor (Claims.ClientApplicationName, user.ClientApplication.Name) |]
      claims.ToAsyncEnumerable () |> baseClaims.Concat
    | _ -> baseClaims
