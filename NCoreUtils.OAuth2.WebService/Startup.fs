namespace NCoreUtils.OAuth2.WebService

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.Authentication
open NCoreUtils.ContentDetection
open NCoreUtils.Data
open NCoreUtils.Features
open NCoreUtils.Images
open NCoreUtils.Logging
open NCoreUtils.OAuth2
open NCoreUtils.OAuth2.Data
open NCoreUtils.Storage
open NCoreUtils.Text
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

module RAV = NCoreUtils.OAuth2.RestAccessValidation

type Startup (env: IHostingEnvironment) =

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
      | _ ->
        printfn "Using ENV to provide configuration"
        EnvConfig.buildConfigFromEnv ()

    let googleBucketConfiguration =
      configuration.GetSection("Google").Get<GoogleBucketConfiguration> ()
      |?? (GoogleBucketConfiguration ())

    let encryptionConfiguration = configuration.GetSection("Google").Get<GoogleEncryptionConfiguration>()
    if isNull (box encryptionConfiguration) then
      eprintfn "Unable to bind GoogleEncryptionConfiguration"

    let googleLoggingConfiguration = configuration.GetSection("Google").Get<GoogleLoggingConfiguration>()
    if isNull (box encryptionConfiguration) then
      eprintfn "Unable to bind GoogleLoggingConfiguration"

    services
#if DEBUG
      .AddSingleton<ITestContextInitializer>({ new ITestContextInitializer with member __.Initialize() = () })
#endif
      .AddSingleton<IConfiguration>(configuration)
      .AddSingleton(encryptionConfiguration)
      .AddSingleton(googleLoggingConfiguration)
      .AddSingleton(configuration.GetSection("OAuth2").Get<OAuth2Configuration>())
      .AddSingleton(googleBucketConfiguration)
      // Logging
      .AddLogging(fun builder ->
        builder
          .ClearProviders()
          .SetMinimumLevel(LogLevel.Information)
          .AddFilter(DbLoggerCategory.Infrastructure.Name, LogLevel.Error)
          .AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning)
          |> ignore
        if env.IsDevelopment ()
          then builder.AddConsole () |> ignore
          else builder.AddGoogleSink(googleLoggingConfiguration) |> ignore
      )
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
      // name simplification (required for file name generation)
      .AddSingleton<ISimplifier>(Simplifier.Default)
      // content detection
      .ConfigureContentDetection(fun b -> b.AddMagic(fun bb -> bb.AddLibmagicRules() |> ignore) |> ignore)
      // image processing
      .AddSingleton(configuration.GetSection("Images").Get<ImageResizerClientConfiguration>())
      .AddImageResizer<ImageResizerClient>()
      // google cloud storage
      .AddTransient<IFeatureCollection<IStorageProvider>>(fun _ -> FeatureCollectionBuilder().Build<IStorageProvider>())
      .AddGoogleCloudStorageProvider(
        configuration.["Google:ProjectId"],
        fun (b : GoogleCloudStorageOptionsBuilder) ->
          b.ChunkSize           <- Nullable.mk 262144;
          b.PredefinedAcl       <- Nullable.mk Google.Cloud.Storage.V1.PredefinedObjectAcl.PublicRead;
          b.DefaultCacheControl <- "max-age=2628000, private"
      )
      // REST access + file upload
      .AddDataQueryServices(fun _ -> ())
      .AddFileUploader(configuration.GetSection "Images", fun path -> Uri (sprintf "gs://%s/%s" googleBucketConfiguration.BucketName (path.Trim '/'), UriKind.Absolute))
      .AddCustomRestPipeline()
      .AddScoped<Rest.CurrentRestTypeName>()
      .ConfigureRest(fun b -> b.WithPathPrefix(CaseInsensitive "/data").ConfigureAccess(configureRestAccess).AddRange [| typeof<User>; typeof<Permission>; typeof<File> |] |> ignore)
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

  member __.Configure (app: IApplicationBuilder, test : ITestContextInitializer) =
    test.Initialize ()

    let forwardedHeaderOptions = ForwardedHeadersOptions (ForwardedHeaders = ForwardedHeaders.All)
    forwardedHeaderOptions.KnownNetworks.Clear();
    forwardedHeaderOptions.KnownProxies.Clear();

    app
      .UseCors(fun builder ->
        builder
          .AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowCredentials()
          .AllowAnyMethod()
          .WithExposedHeaders("X-Access-Token", "X-Total-Count", "Location", "X-Message")
          |> ignore
      )
      // .Use(forceGC)
      // .Use(ProxyMiddleware.run)
      .UseForwardedHeaders(forwardedHeaderOptions)
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