[<AutoOpen>]
module internal NCoreUtils.OAuth2.OAuth2MiddlewareHelpers

open NCoreUtils

let inline (|Eq|_|) (value : string) (ci : CaseInsensitive) =
  match CaseInsensitive value = ci with
  | true -> Some ()
  | _    -> None
