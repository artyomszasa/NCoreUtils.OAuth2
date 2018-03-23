namespace NCoreUtils.OAuth2

open System.Threading
open System.Threading.Tasks
open NCoreUtils

[<Interface>]
type IEncryptionProvider =
  abstract Encrypt : plainData:byte[] -> Async<byte[]>
  abstract Decrypt : cipherData:byte[] -> Async<byte[]>

[<AbstractClass>]
type AsyncEncryptionProvider () =

  abstract EncryptAsync : plainData:byte[]  * cancellationToken:CancellationToken -> Task<byte[]>
  abstract DecryptAsync : cipherData:byte[] * cancellationToken:CancellationToken -> Task<byte[]>
  interface IEncryptionProvider with
    member this.Encrypt plainData  = Async.Adapt (fun cancellationToken -> this.EncryptAsync (plainData, cancellationToken))
    member this.Decrypt cipherData = Async.Adapt (fun cancellationToken -> this.DecryptAsync (cipherData, cancellationToken))