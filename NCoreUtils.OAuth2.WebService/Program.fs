namespace NCoreUtils.OAuth2.WebService

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
// open NCoreUtils.AspNetCore
// open NCoreUtils.Posix

module Program =
  let exitCode = 0

  let BuildWebHost (args : string[]) =
    WebHost.CreateDefaultBuilder(args)
      //.UseNCorePosixServer(args)
      .UseKestrel()
      .UseStartup<Startup>()
      .Build()

  [<EntryPoint>]
  let main args =
    BuildWebHost(args).Run ()
    // EpollManager.Stop ()
    exitCode
