namespace NCoreUtils.OAuth2
(*

open System
open System.ComponentModel
open System.Reflection
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives
open Microsoft.FSharp.Reflection
open NCoreUtils
open Newtonsoft.Json

[<AutoOpen>]
module FSharpMiddleware =

  type IApplicationBuilder with
    member this.Use (action : HttpContext -> Async<unit> option) =
      let func =
        Func<HttpContext, Func<Task>, Task>
          (fun httpContext next ->
            match action httpContext with
            | Some computation -> Async.StartAsTask (computation, cancellationToken = httpContext.RequestAborted) :> _
            | _                -> next.Invoke ())
      this.Use func

exception MissingParameterException of ParameterName:string

type OAuth2ErrorResult = {
  [<JsonProperty("error")>]
  Error            : OAuth2Error
  [<JsonProperty("error_description")>]
  ErrorDescription : string }

[<AutoOpen>]
module CustomAttributeProviderExt =

  type ICustomAttributeProvider with
    member this.TryGetAttribute<'attribute when 'attribute :> Attribute> (?``inherit``) =
      let attrs = this.GetCustomAttributes (typeof<'attribute>, defaultArg ``inherit`` true)
      match attrs.Length with
      | 0 -> None
      | _ -> Some (attrs.[0] :?> 'attribute)

type IParameterBinder =
  abstract BindParameter : name:string * target:ICustomAttributeProvider * getParameter:(string -> string option) -> Async<obj>

type StringParameterBinder () =
  interface IParameterBinder with
    member __.BindParameter (name, target, getParameter) =
      let value =
        match getParameter name with
        | Some value -> box value
        | _ ->
          match target.TryGetAttribute<DefaultValueAttribute> () with
          | None      -> MissingParameterException name |> raise
          | Some attr -> attr.Value
      async.Return value

[<AttributeUsage(AttributeTargets.Property)>]
type BinderAttribute (binderType : Type) =
  inherit Attribute ()
  member val BinderType = binderType

[<AttributeUsage(AttributeTargets.Property)>]
type NameAttribute (name : string) =
  inherit Attribute ()
  member val Name = name

type HttpMethod =
  | HttpGet
  | HttpPost
  | HttpOptions
  | HttpPut
  | HttpDelete
  | HttpHead
  | HttpCustom of Method:CaseInsensitive

module HttpMethod =

  let private wellKnownMethods =
    Map.ofList
      [ CaseInsensitive "GET",     HttpGet
        CaseInsensitive "POST",    HttpPost
        CaseInsensitive "OPTIONS", HttpOptions
        CaseInsensitive "PUT",     HttpPut
        CaseInsensitive "DELETE",  HttpDelete
        CaseInsensitive "HEAD",    HttpHead ]

  let parse (input : string) =
    let ci = CaseInsensitive input
    match Map.tryFind ci wellKnownMethods with
    | Some m -> m
    | _      -> HttpCustom ci

[<AutoOpen>]
module AspNetCoreExt =

  let private utf8 = UTF8Encoding false


  let path (httpContext : HttpContext) =
    match httpContext.Request.Path with
    | path when path.HasValue -> path.Value.Trim('/').Split '/' |> Seq.map CaseInsensitive |> List.ofSeq
    | _                       -> []

  let httpMethod (httpContext : HttpContext) =
    httpContext.Request.Method |> HttpMethod.parse

  let getQueryParameter (httpContext : HttpContext) name =
    let mutable values = Unchecked.defaultof<_>
    match httpContext.Request.Query.TryGetValue (name, &values) with
    | true when values.Count > 0 -> Some <| values.[0]
    | _                          -> None

  let getFormParameter (httpContext : HttpContext) name =
    let mutable values = Unchecked.defaultof<_>
    match httpContext.Request.Form.TryGetValue (name, &values) with
    | true when values.Count > 0 -> Some <| values.[0]
    | _                          -> None

  [<RequiresExplicitTypeArguments>]
  let bindObjectServices<'a> (httpContext : HttpContext) =
    FSharpType.GetRecordFields typeof<'a>
    |> Array.map (fun prop -> httpContext.RequestServices.GetService prop.PropertyType)
    |> (fun values -> FSharpValue.MakeRecord (typeof<'a>, values))
    :?> 'a

  [<RequiresExplicitTypeArguments>]
  let bindObjectParameters<'a> (httpContext : HttpContext) getParameter =
    FSharpType.GetRecordFields typeof<'a>
    |> Array.map
      (fun prop ->
        match prop.TryGetAttribute<BinderAttribute> () with
        | None      -> failwithf "%s.%s has no associated attribute" prop.DeclaringType.FullName prop.Name
        | Some attr ->
          let name =
            match prop.TryGetAttribute<NameAttribute> () with
            | None      -> prop.Name
            | Some attr -> attr.Name
          let binder = ActivatorUtilities.CreateInstance (httpContext.RequestServices, attr.BinderType) :?> IParameterBinder
          binder.BindParameter (name, prop, getParameter)
      )
    |> Async.Sequential
    >>| (fun values -> FSharpValue.MakeRecord (typeof<'a>, values) :?> 'a)

  let bindServices httpContext (f : 'a -> 'b) =
    bindObjectServices<'a> httpContext
    |> f

  let bindParameters httpContext getParameter (f : 'a -> 'b) =
    bindObjectParameters<'a> httpContext getParameter
    >>| f

  let bindAndExecute (httpContext : HttpContext) getParameter f =
    async.Bind (
      f httpContext
      |> bindServices httpContext
      |> bindParameters httpContext getParameter,
      id)

  let json (httpContext : HttpContext) obj =
    let data =
      Newtonsoft.Json.JsonConvert.SerializeObject obj
      |> utf8.GetBytes
    let response = httpContext.Response
    response.Headers.["Content-Type"] <- StringValues "application/json; charset=utf-8"
    response.Headers.ContentLength    <- Nullable.mk data.LongLength
    response.Body.AsyncWrite data
*)