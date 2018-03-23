namespace NCoreUtils.OAuth2

open System
open System.Runtime.Serialization
open System.Runtime.InteropServices
open System.Runtime.CompilerServices

[<AutoOpen>]
module private OAuth2ExceptionKeys =
  [<Literal>]
  let KeyError = "oauth2.error"
  [<Literal>]
  let KeyErrorDescription = "oauth2.errorDescription"
  [<Literal>]
  let BaseMessage = "OAuth2 exception has been thrown."


[<Serializable>]
type OAuth2Exception =
  inherit Exception
  val mutable private error : OAuth2Error
  val mutable private errorDescription : string

  member this.Error
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.error

  member this.ErrorDescription
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.errorDescription

  new (info : SerializationInfo, context) =
    { inherit Exception (info, context)
      error            = enum<OAuth2Error> (info.GetInt32 KeyError)
      errorDescription = info.GetString KeyErrorDescription }

  new (error, [<Optional>] errorDescription, [<Optional>] innerException) =
    { inherit Exception (BaseMessage, innerException)
      error = error
      errorDescription = errorDescription }

  new (error, innerException) = OAuth2Exception(error, null, innerException)

  override this.GetObjectData (info, context) =
    base.GetObjectData (info, context)
    info.AddValue (KeyError,            int this.error)
    info.AddValue (KeyErrorDescription, this.errorDescription)

[<AutoOpen>]
module OAuth2ExceptionExt =

  let (|OAuth2Exception|_|) (exn : exn) =
    match exn with
    | :? OAuth2Exception as e -> Some (e.Error, e.ErrorDescription)
    | _                       -> None

  let oauth2error error (message : string) = OAuth2Exception (error, message) |> raise

  let oauth2errorf error fmt = Printf.kprintf (oauth2error error) fmt