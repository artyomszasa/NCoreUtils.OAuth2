namespace NCoreUtils.OAuth2

open System.Security.Cryptography

type AesEncryptionProvider (configuration : AesEncryptionConfiguration) =

  member __.Decrypt cipherData =
    use aes = Aes.Create ()
    aes.Key <- configuration.KeyData
    aes.IV  <- configuration.IVData
    aes.CreateDecryptor().TransformFinalBlock (cipherData, 0, cipherData.Length)

  member __.Encrypt plainData =
    use aes = Aes.Create ()
    aes.Key <- configuration.KeyData
    aes.IV  <- configuration.IVData
    aes.CreateEncryptor().TransformFinalBlock (plainData, 0, plainData.Length)

  interface IEncryptionProvider with

    member this.Decrypt cipherData =
      this.Decrypt cipherData |> async.Return

    member this.Encrypt plainData =
      this.Encrypt plainData |> async.Return
