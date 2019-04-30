namespace NCoreUtils.OAuth2

open NCoreUtils

[<NoEquality; NoComparison>]
type RefreshTokenParameters = {
  [<ParameterName(OAuth2Parameters.RefreshToken)>]
  RefreshToken : string }
