namespace NCoreUtils.OAuth2.WebService

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging
open NCoreUtils.AspNetCore
open NCoreUtils.Authentication
open NCoreUtils.OAuth2
open NCoreUtils.OAuth2.Data
open System.Threading
open NCoreUtils.Logging

type Startup() =

  static let send404 =
    RequestDelegate
      (fun context ->
        context.Response.StatusCode <- 404
        Task.CompletedTask)

  let mutable counter = 0

  let forceGC (_ : HttpContext) (next : Func<Task>) =
    match Interlocked.Increment (&counter) % 32 with
    | 0 ->
      GC.Collect ()
      GC.WaitForPendingFinalizers ()
    | _ -> ()
    next.Invoke ()

  member __.ConfigureServices (services: IServiceCollection) =
    let configuration =
      ConfigurationBuilder()
        .SetBasePath(System.IO.Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = false, reloadOnChange = false)
        .Build ()

    services
      .AddSingleton<IConfiguration>(configuration)
      .AddSingleton(configuration.GetSection("Google").Get<GoogleEncryptionConfiguration>())
      .AddSingleton(configuration.GetSection("Google").Get<GoogleLoggingConfiguration>())
      // Logging
      .AddLogging(fun builder -> builder.ClearProviders().SetMinimumLevel(LogLevel.Trace) |> ignore)
      .AddOAuth2DbContext(fun builder -> builder.UseNpgsql(configuration.GetConnectionString("Default"), fun b -> b.MigrationsAssembly("NCoreUtils.OAuth2.Data.EntityFrameworkCore") |> ignore) |> ignore)
      .AddOAuth2DataRepositories()
      // http context access
      .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
      // Encryption provider
      .AddSingleton<IEncryptionProvider, GoogleEncryptionProvider>()
      // Session level client application id
      .AddScoped<CurrentClientApplication>()
      // login provider
      .AddLoginAuthenticator(fun b -> b.AddPasswordAuthentication<OAuth2UserManager, int, OAuth2PasswordLogin>() |> ignore)
      // core functions
      .AddScoped<IOAuth2Core, OAuth2Core>()
      |> ignore

    services
      // user manager (if not already registered)
      .TryAddTransient<OAuth2UserManager>()
      |> ignore

    ()

  member __.Configure (app: IApplicationBuilder, env: IHostingEnvironment, loggerFactory : ILoggerFactory, googleLoggingConfiguration : GoogleLoggingConfiguration) =
    if env.IsDevelopment()
      then loggerFactory.AddConsole().AddDebug() |> ignore
      else loggerFactory .AddGoogle(googleLoggingConfiguration)
    app
      .Use(forceGC)
      .Use(OAuth2Middleware.run)
      .Run(send404)
    ()