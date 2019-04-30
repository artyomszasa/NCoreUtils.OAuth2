namespace NCoreUtils.OAuth2.WebService

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting

module Program =
  let exitCode = 0

  let CreateWebHostBuilder (args : string[]) =
    WebHost.CreateDefaultBuilder(args)
      .UseKestrel()
      .UseStartup<Startup>()

  [<EntryPoint>]
  let main args =
    CreateWebHostBuilder(args).Build().Run ()
    exitCode
