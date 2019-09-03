namespace NCoreUtils.OAuth2

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open NCoreUtils.Authentication
open NCoreUtils.OAuth2.Data


type OAuth2PasswordLogin (serviceProvider : IServiceProvider,
                          userManager     : OAuth2UserManager,
                          logger          : ILogger<OAuth2PasswordLogin>) =
  inherit PasswordLogin<int> (userManager,
                              logger :?> ILogger<PasswordLogin<int>>,
                              serviceProvider.GetService<PasswordLoginOptions> (),
                              serviceProvider.GetService<IUsernameFormatter> ())

  static let asAsyncEnumerator (enumerator : Collections.Generic.IEnumerator<_>) =
    { new Collections.Generic.IAsyncEnumerator<_> with
        member __.Current = enumerator.Current
        member __.MoveNextAsync () = ValueTask<_> (enumerator.MoveNext ())
        member __.DisposeAsync () =
          enumerator.Dispose ()
          Unchecked.defaultof<_>
    }

  static let asAsyncEnumerable (enumerable : Collections.Generic.IEnumerable<_>) =
    { new Collections.Generic.IAsyncEnumerable<_> with
        member __.GetAsyncEnumerator _ = asAsyncEnumerator <| enumerable.GetEnumerator ()
    }

  override __.GetClaimsAsync (user, forceName) =
    let baseClaims = base.GetClaimsAsync(user, forceName)
    match user with
    | :? User as user ->
      let claims =
        match obj.ReferenceEquals (user.ClientApplication, null) with
        | true ->
          [|  ClaimDescriptor (Claims.ClientApplicationId, user.ClientApplicationId.ToString ()) |]
        | _ ->
          [|  ClaimDescriptor (Claims.ClientApplicationId,   user.ClientApplication.Id.ToString ())
              ClaimDescriptor (Claims.ClientApplicationName, user.ClientApplication.Name) |]
      NCoreUtils.AsyncLinqExtensions.Concat(asAsyncEnumerable claims, baseClaims)
    | _ -> baseClaims
