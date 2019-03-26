namespace NCoreUtils.OAuth2

open System

type AesEncryptionConfiguration () =
  let mutable key = null : byte[]
  let mutable iv  = null : byte[]
  /// Base64 encoded key.
  member val Key = Unchecked.defaultof<string> with get, set
  /// Base64 encoded initialization vector.
  member val IV  = Unchecked.defaultof<string> with get, set
  member internal this.KeyData =
    if isNull key then
      match String.IsNullOrEmpty this.Key with
      | true -> invalidOp "Empty key."
      | _    -> key <- Convert.FromBase64String this.Key
    key
  member internal this.IVData =
    if isNull iv then
      match String.IsNullOrEmpty this.IV with
      | true -> invalidOp "Empty key."
      | _    -> iv <- Convert.FromBase64String this.IV
    iv