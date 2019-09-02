namespace NCoreUtils.OAuth2.WebService

open System
open System.Net
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting

module Program =
  let exitCode = 0

  [<CompiledName("ParseEndpoint")>]
  let private parseEndpoint input =
    match input with
    | null | "" -> IPEndPoint (IPAddress.Loopback, 5000) // DEFAULT HOST+PORT
    | _ ->
      match input.LastIndexOf ':' with
      | -1        -> IPEndPoint (IPAddress.Parse input, 5000)
      | portIndex ->
        IPEndPoint (
          IPAddress.Parse (input.Substring (0, portIndex)),
          Int32.Parse (input.Substring (portIndex + 1))
        )

  let CreateWebHostBuilder (args : string[]) =
    WebHostBuilder()
      .UseContentRoot(IO.Directory.GetCurrentDirectory ())
      .UseKestrel(fun o -> o.AllowSynchronousIO <- true)
      .UseStartup<Startup>()

  [<EntryPoint>]
  let main args =
    CreateWebHostBuilder(args).Build().Run ()
    exitCode
