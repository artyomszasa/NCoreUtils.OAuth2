namespace NCoreUtils.OAuth2.Rest

open NCoreUtils
open NCoreUtils.AspNetCore.Rest
open NCoreUtils.OAuth2.Data
open NCoreUtils.AspNetCore
open NCoreUtils.ContentDetection
open System.Reflection

type FileUpload () =
  inherit File ()
  member val Data = Unchecked.defaultof<byte[]> with get, set

[<Interface>]
type IDefaultRestFileCreate =
  inherit IRestCreate<File, int>

[<Interface>]
type IFileUploadDeserializer =
  inherit IDeserializer<FileUpload>

type CurrentUploadData () =
  member val Data = Unchecked.defaultof<byte[]> with get, set

// ***********************************************************
// File upload (w data) deserializer

[<AutoOpen>]
module private FileDeserializerHelpers =

  let fileProperties = typeof<File>.GetProperties (BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.FlattenHierarchy)

type FileDeserializer =
  val private defaultDeserializer : IFileUploadDeserializer
  val private uploadData          : CurrentUploadData
  new (defaultDeserializer, uploadData) =
    { defaultDeserializer = defaultDeserializer
      uploadData          = uploadData }
  interface IDeserializer<File> with
    member this.Deserialize stream =
      let upload = this.defaultDeserializer.Deserialize stream
      this.uploadData.Data <- upload.Data
      let file = File ()
      for fileProperty in fileProperties do
        fileProperty.SetValue (file, fileProperty.GetValue (upload, null), null)
      file

// ***********************************************************
// REST File creation

type RestFileCreate =
  val private defaultCreate   : IDefaultRestFileCreate
  val private uploadData      : CurrentUploadData
  val private contentAnalyzer : IContentAnalyzer
  val private fileUploader    : IFileUploader

  new (defaultCreate, uploadData, contentAnalyzer, fileUploader) =
    { defaultCreate   = defaultCreate
      uploadData      = uploadData
      contentAnalyzer = contentAnalyzer
      fileUploader    = fileUploader }

  interface IRestCreate<File, int> with
    member this.AsyncBeginTransaction () = this.defaultCreate.AsyncBeginTransaction ()
    member this.AsyncInvoke file = async {
      match this.uploadData.Data with
      | null                      -> return BadRequestException "File being uploaded without content."    |> raise
      | data when data.Length = 0 -> return BadRequestException "File being uploaded with empty content." |> raise
      | data ->
      match file.OriginalName with
      | null | "" -> return BadRequestException "Unable to upload unnamed image." |> raise
      | _ ->
        let! contentInfo = Async.Adapt (fun cancellationToken -> this.contentAnalyzer.Analyze (data, cancellationToken))
        file.MediaType <- contentInfo.MediaType |?? "application/octet-stream"
        let! res = this.defaultCreate.AsyncInvoke file
        do! this.fileUploader.AsyncUpload (res, data)
        return res }
