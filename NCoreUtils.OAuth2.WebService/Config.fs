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

let private vars =
  [ vx "DB_HOST"                   (Some "127.0.0.1")   None
    vx "DB_PORT"                   (Some "5432")        None
    vx "DB_DATABASE"               (Some "oauth2")      None
    vx "DB_USER"                    None                None
    vy "DB_PASSWORD"                None                None
    vx "AES_KEY"                    None                None
    vx "AES_IV"                     None                None
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
    |> add "Aes:Key"                         (vv "AES_KEY")
    |> add "Aes:IV"                          (vv "AES_IV")
    |> add "OAuth2:AccessTokenExpiry"        (vv "ACCESS_TOKEN_EXPIRY")
    |> add "OAuth2:AuthorizationCodeExpiry"  (vv "AUTHORIZATION_CODE_EXPIRY")
    |> add "OAuth2:RefreshTokenExpiry"       (vv "REFRESH_TOKEN_EXPIRY")
  ConfigurationBuilder().AddInMemoryCollection(dict).Build ()

