namespace NCoreUtils.OAuth2.WebService

open Microsoft.AspNetCore.Hosting
open NCoreUtils.AspNetCore
// open NCoreUtils.Posix

module Program =
  let exitCode = 0

  let BuildWebHost (args : string[]) =
    WebHostBuilder()
      //.UseNCorePosixServer(args)
      .UseKestrel(args)
      .UseStartup<Startup>()
      .Build()

  [<EntryPoint>]
  let main args =
    BuildWebHost(args).Run ()
    // EpollManager.Stop ()
    exitCode
