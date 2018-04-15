module NCoreUtils.OAuth2.WebService.EnvConfig

open System.Net.Http
open System.Text.RegularExpressions
open NCoreUtils
open System
open System.Collections.Generic
open Microsoft.Extensions.Configuration

exception InitializationException of Msg:string
  with
    override this.Message =
      match this :> exn with
      | InitializationException msg -> msg
      | _                           -> Unchecked.defaultof<_>

type private Var = {
  Name                : string
  DefaultValue        : string option
  DefaultValueFactory : (unit -> string option) option
  IsSensitive         : bool }

let private v name def f isSensitive =
  { Name                = name
    DefaultValue        = def
    DefaultValueFactory = f
    IsSensitive         = isSensitive }

let private vx name def f = v name def f false

let private vy name def f = v name def f true

let private getLocation =
  let regex = Regex "(europe|us|asia)-[a-z]+[0-9]"
  fun (str : string) -> regex.Match(str).Groups.[0].Value

let private getGoogleMetadata (uri : string) =
  try
    use client = new HttpClient ()
    use request = new HttpRequestMessage (HttpMethod.Get, uri)
    request.Headers.Add ("Metadata-Flavor", "Google")
    use response = client.SendAsync(request).Result
    response.Content.ReadAsStringAsync().Result |> Some
  with exn ->
    eprintfn "Unable to get google metadata (uri = %s): %s" uri exn.Message
    None


let private vars =
  [ vx "DB_HOST"                   (Some "127.0.0.1")   None
    vx "DB_PORT"                   (Some "5432")        None
    vx "DB_DATABASE"               (Some "oauth2")      None
    vx "DB_USER"                    None                None
    vy "DB_PASSWORD"                None                None
    vx "GOOGLE_PROJECTID"           None               (Some (fun () -> getGoogleMetadata "http://metadata.google.internal/computeMetadata/v1/project/project-id"))
    vx "GOOGLE_LOCATIONID"          None               (Some (fun () -> getGoogleMetadata "http://metadata.google.internal/computeMetadata/v1/instance/zone" >>| getLocation))
    vx "GOOGLE_SERVICENAME"         None                None
    vx "GOOGLE_KEYRINGID"           None                None
    vx "GOOGLE_KEYID"               None                None
    vx "ACCESS_TOKEN_EXPIRY"       (Some "00:15:00")    None
    vx "AUTHORIZATION_CODE_EXPIRY" (Some "00:10:00")    None
    vx "REFRESH_TOKEN_EXPIRY"      (Some "45.00:00:00") None ]

let buildConfigFromEnv () =
  let map =
    vars
    |> Seq.fold
      (fun map var ->
        match Environment.GetEnvironmentVariable var.Name with
        | null ->
          match var.DefaultValue with
          | Some v ->
            match var.IsSensitive with
            | true -> printfn "%s not defined in environment, using default value." var.Name
            | _    -> printfn "%s not defined in environment, using default value => \"%s\"." var.Name v
            Map.add var.Name v map
          | _ ->
          match var.DefaultValueFactory >>= (fun f -> f ()) with
          | Some v ->
            match var.IsSensitive with
            | true -> printfn "%s not defined in environment, using generated value." var.Name
            | _    -> printfn "%s not defined in environment, using generated value => \"%s\"." var.Name v
            Map.add var.Name v map
          | _ ->
            sprintf "Unable to get value for %s." var.Name
            |> InitializationException
            |> raise
        | v ->
          match var.IsSensitive with
          | true -> printfn "Using environment value for %s." var.Name
          | _    -> printfn "Using environment value for %s. => \"%s\"." var.Name v
          Map.add var.Name v map
      )
      Map.empty
  let vv key = Map.find key map
  let dict =
    let inline add k v (d : Dictionary<string, string>) =
      d.Add (k, v)
      d
    Dictionary ()
    |> add "ConnectionStrings:Default"       (sprintf "Host=%s; Port=%s; Username=%s; Password=%s;Database=%s" (vv "DB_HOST") (vv "DB_PORT") (vv "DB_USER") (vv "DB_PASSWORD") (vv "DB_DATABASE"))
    |> add "Google:ProjectId"                (vv "GOOGLE_PROJECTID")
    |> add "Google:ServiceName"              (vv "GOOGLE_SERVICENAME")
    |> add "Google:LocationId"               (vv "GOOGLE_LOCATIONID")
    |> add "Google:KeyRingid"                (vv "GOOGLE_KEYRINGID")
    |> add "Google:KeyId"                    (vv "GOOGLE_KEYID")
    |> add "OAuth2:AccessTokenExpiry"        (vv "ACCESS_TOKEN_EXPIRY")
    |> add "OAuth2:AuthorizationCodeExpiry"  (vv "AUTHORIZATION_CODE_EXPIRY")
    |> add "OAuth2:RefreshTokenExpiry"       (vv "REFRESH_TOKEN_EXPIRY")
  ConfigurationBuilder().AddInMemoryCollection(dict).Build ()

