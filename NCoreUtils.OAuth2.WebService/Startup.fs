namespace NCoreUtils.OAuth2.WebService

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.Authentication
open NCoreUtils.Logging
open NCoreUtils.OAuth2
open NCoreUtils.OAuth2.Data
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

module RAV = NCoreUtils.OAuth2.RestAccessValidation

type Startup() =

  static let send404 =
    RequestDelegate
      (fun context ->
        context.Response.StatusCode <- 404
        Task.CompletedTask)

  static let configureRestAccess (builder : Rest.RestAccessConfigurationBuilder) =
    builder.ConfigureCreate(RAV.create)
      .ConfigureUpdate(RAV.update)
      .ConfigureDelete(RAV.delete)
      .ConfigureItem(RAV.item)
      .ConfigureList(RAV.list)

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

      let appsettingsPath = Path.Combine (System.IO.Directory.GetCurrentDirectory (), "appsettings.json")
      match File.Exists appsettingsPath with
      | true ->
        ConfigurationBuilder()
          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional = false, reloadOnChange = false)
          .Build ()
      | _ -> EnvConfig.buildConfigFromEnv ()

    services
      .AddSingleton<IConfiguration>(configuration)
      .AddSingleton(configuration.GetSection("Google").Get<GoogleEncryptionConfiguration>())
      .AddSingleton(configuration.GetSection("Google").Get<GoogleLoggingConfiguration>())
      .AddSingleton(configuration.GetSection("OAuth2").Get<OAuth2Configuration>())
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
      // REST access
      .AddScoped<Rest.CurrentRestTypeName>()
      .ConfigureRest(fun b -> b.WithPathPrefix(CaseInsensitive "/data").ConfigureAccess(configureRestAccess).AddRange [| typeof<User>; typeof<Permission> |] |> ignore)
      // REST access validation helper
      .AddScoped<InternalUserInfo>()
      // Global JsonSerializerSettings
      .AddSingleton(JsonSerializerSettings (ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = CamelCasePropertyNamesContractResolver ()))
      // CORS
      .AddCors()
      // auth
      .AddAuthentication("internal").AddScheme<InternalAuthenticationSchemeOtions, InternalAuthenticationHandler>("internal", Action<_> ignore)
      |> ignore

    services
      // user manager (if not already registered)
      .TryAddTransient<OAuth2UserManager>()
      |> ignore

    ()

  member __.Configure (app: IApplicationBuilder, env: IHostingEnvironment, loggerFactory : ILoggerFactory, googleLoggingConfiguration : GoogleLoggingConfiguration, httpContextAccessor) =
    //let struct (hash, salt) = PasswordLogin.GeneratePaswordHash "39c32190"
    let struct (hash, salt) = PasswordLogin.GeneratePaswordHash "3983a190"
    printfn "Salt = %s" salt
    printfn "Hash = %s" hash
    Environment.Exit 0

    if env.IsDevelopment()
      then loggerFactory.AddConsole(LogLevel.Trace).AddDebug(LogLevel.Trace) |> ignore
      else loggerFactory.AddGoogleSink(httpContextAccessor, googleLoggingConfiguration) |> ignore
    app
      .UseCors(fun builder -> builder.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod().WithExposedHeaders("X-Access-Token", "X-Total-Count", "Location", "X-Message") |> ignore)
      .Use(forceGC)
      .Use(OAuth2Middleware.run)
      .UseRequestErrorHandler()
      .UseAuthentication()
      .UseRest()
      .Run(send404)
    ()