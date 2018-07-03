namespace NCoreUtils.OAuth2.WebService

open System
open System.IO
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
open NCoreUtils.Data
open NCoreUtils.Logging
open NCoreUtils.OAuth2
open NCoreUtils.OAuth2.Data
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
      let appsettingsSecretPath = Path.Combine (System.IO.Directory.GetCurrentDirectory (), "secrets", "appsettings.json")
      match File.Exists appsettingsPath with
      | true ->
        ConfigurationBuilder()
          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
          .AddJsonFile(appsettingsPath, optional = false, reloadOnChange = false)
          .Build ()
      | _ ->
      match File.Exists appsettingsSecretPath with
      | true ->
        ConfigurationBuilder()
          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
          .AddJsonFile(appsettingsSecretPath, optional = false, reloadOnChange = false)
          .Build ()
      | _ -> EnvConfig.buildConfigFromEnv ()

    services
      .AddSingleton<IConfiguration>(configuration)
      .AddSingleton(configuration.GetSection("Google").Get<GoogleEncryptionConfiguration>())
      .AddSingleton(configuration.GetSection("Google").Get<GoogleLoggingConfiguration>())
      .AddSingleton(configuration.GetSection("OAuth2").Get<OAuth2Configuration>())
      // Logging
      .AddLogging(fun builder -> builder.ClearProviders().SetMinimumLevel(LogLevel.Trace) |> ignore)
      .AddPrePopulatedLoggingContext()
      // data
      .AddOAuth2DbContext(fun builder -> builder.UseNpgsql(configuration.GetConnectionString("Default"), fun b -> b.MigrationsAssembly("NCoreUtils.OAuth2.Data.EntityFrameworkCore") |> ignore) |> ignore)
      .AddOAuth2DataRepositories()
      // password encoding
      .AddSingleton({ new IPasswordEncryption with member __.EncryptPassword p = let struct (hash, salt) = PasswordLogin.GeneratePaswordHash p in { PasswordHash = hash; Salt = salt }})
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
    // let struct (hash, salt) = PasswordLogin.GeneratePaswordHash "39c32190"
    // let struct (hash0, salt0) = PasswordLogin.GeneratePaswordHash "56a563bedb"
    // printfn "Salt = %s" salt0
    // printfn "Hash = %s" hash0
    // let struct (hash1, salt1) = PasswordLogin.GeneratePaswordHash "43e894d3c4"
    // printfn "Salt = %s" salt1
    // printfn "Hash = %s" hash1
    // let struct (hash2, salt2) = PasswordLogin.GeneratePaswordHash "e0e54deb05"
    // printfn "Salt = %s" salt2
    // printfn "Hash = %s" hash2
    // Environment.Exit 0

    if env.IsDevelopment()
      then loggerFactory.AddConsole(LogLevel.Trace).AddDebug(LogLevel.Trace) |> ignore
      else loggerFactory.AddGoogleSink(httpContextAccessor, googleLoggingConfiguration) |> ignore
    app
      .UseCors(fun builder -> builder.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod().WithExposedHeaders("X-Access-Token", "X-Total-Count", "Location", "X-Message") |> ignore)
      // .Use(forceGC)
      .UsePrePopulateLoggingContext()
      .Use(OAuth2Middleware.run)
      .UseRequestErrorHandler()
      .UseAuthentication()
      .Use(fun httpContext asyncNext -> async {
        let  currentClientApplication = httpContext.RequestServices.GetRequiredService<CurrentClientApplication> ()
        let  logger = httpContext.RequestServices.GetRequiredService<ILogger<Startup>> ()
        let  repo = httpContext.RequestServices.GetRequiredService<IDataRepository<ClientApplication>> ()
        let! clientApplication = CurrentClientApplicationResolver.resolveClientApplication logger repo httpContext
        do
          match box clientApplication with
          | null -> ()
          | _    -> currentClientApplication.Id <- clientApplication.Id
        return! asyncNext
      })
      .UseRest()
      .Run(send404)
    ()