namespace NCoreUtils.OAuth2.WebService

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

type Startup() =

  static let send404 =
    RequestDelegate
      (fun context ->
        context.Response.StatusCode <- 404
        Task.CompletedTask)

  member __.ConfigureServices (services: IServiceCollection) =
    let configuration =
      ConfigurationBuilder()
        .SetBasePath(System.IO.Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = false, reloadOnChange = false)
        .Build ()

    services
      .AddSingleton<IConfiguration>(configuration)
      .AddSingleton<GoogleEncryptionConfiguration>(configuration.GetSection("Google").Get<GoogleEncryptionConfiguration>())
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



  member __.Configure (app: IApplicationBuilder, env: IHostingEnvironment, loggerFactory : ILoggerFactory) =
    if env.IsDevelopment()
      then
        loggerFactory
          .AddConsole()
          .AddDebug()
          |> ignore

    app
      .Use(OAuth2Middleware.run)
      .Run(send404)

    ()