namespace NCoreUtils.OAuth2.WebService

open Microsoft.AspNetCore.Hosting
open NCoreUtils.Net.AspNetCore
open NCoreUtils.Posix

module Program =
  let exitCode = 0

  let BuildWebHost (_args : string[]) =
    WebHostBuilder()
      .UseNCoreServer(fun builder -> builder.ListenPosix("0.0.0.0", 5000) |> ignore)
      .UseStartup<Startup>()
      .Build()

  [<EntryPoint>]
  let main args =
    BuildWebHost(args).Run ()
    EpollManager.Stop ()
    exitCode
