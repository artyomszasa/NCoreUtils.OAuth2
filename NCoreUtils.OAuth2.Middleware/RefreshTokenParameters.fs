namespace NCoreUtils.OAuth2

open NCoreUtils

type RefreshTokenParameters = {
  [<ParameterName(OAuth2Parameters.RefreshToken)>]
  RefreshToken : string }
