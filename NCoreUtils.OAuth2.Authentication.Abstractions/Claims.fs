[<RequireQualifiedAccess>]
module NCoreUtils.OAuth2.Claims

open NCoreUtils.Authentication

[<Literal>]
let ClientApplicationId = "http://skape.io/claims/clientapplicationid"

[<Literal>]
let ClientApplicationName = "http://skape.io/claims/clientapplicationname"

let inline private claimValue (claim : ClaimDescriptor) = claim.Value

let inline private inlineMap f o =
  match o with
  | Some x -> Some (f x)
  | _      -> None

[<CompiledName("TryGetClaimValue")>]
let tryClaimValue claimType (claims : seq<ClaimDescriptor>) =
  claims
  |> Seq.tryFind (fun claim -> claim.Type = claimType)
  |> inlineMap claimValue