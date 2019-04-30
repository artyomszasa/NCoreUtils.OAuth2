namespace NCoreUtils.OAuth2

open Newtonsoft.Json

type internal OAuth2ErrorConverter () =
  inherit JsonConverter ()
  override __.CanConvert ``type`` = ``type`` = typeof<OAuth2Error>
  override __.WriteJson (writer, value, _serializer) =
    match value with
    | null -> writer.WriteNull ()
    | :? OAuth2Error as error -> OAuth2Error.stringify error |> writer.WriteValue
    | _ -> invalidOp "should never happen"
  override __.ReadJson (reader, _objectType, _existingValue, _serializer) =
    match reader.TokenType with
    | JsonToken.String  -> reader.Value :?> string |> OAuth2Error.parse |> box
    | JsonToken.Integer -> System.Convert.ToInt32 reader.Value |> enum<OAuth2Error> |> box
    | token -> JsonSerializationException (sprintf "Unable to convert %A to OAuth2Error" token) |> raise

[<StructuralEquality; NoComparison>]
type OAuth2ErrorResult = {
  [<JsonProperty("error"); JsonConverter(typeof<OAuth2ErrorConverter>)>]
  Error            : OAuth2Error
  [<JsonProperty("error_description")>]
  ErrorDescription : string }
