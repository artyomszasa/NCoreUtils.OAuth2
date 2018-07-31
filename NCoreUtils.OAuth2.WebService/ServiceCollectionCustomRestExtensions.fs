namespace NCoreUtils.OAuth2

open Microsoft.Extensions.DependencyInjection
open NCoreUtils.AspNetCore.Rest
open NCoreUtils.OAuth2.Data
open NCoreUtils.OAuth2.Rest
open System
open System.Runtime.CompilerServices

[<Extension>]
[<AbstractClass; Sealed>]
type ServiceCollectionRestFileExtensions private () =

  static let createDefaultRestFileCreate =
    Func<IServiceProvider, _>
      (fun serviceProvider ->
        let instance = ActivatorUtilities.CreateInstance<DefaultRestCreate<File, int>> serviceProvider
        { new IDefaultRestFileCreate with
            member __.AsyncBeginTransaction () = instance.AsyncBeginTransaction ()
            member __.AsyncInvoke data = instance.AsyncInvoke data
        })

  static let createFileUploadDeserializer =
    Func<IServiceProvider, _>
      (fun serviceProvider ->
        let instance = ActivatorUtilities.CreateInstance<DefaultDeserializer<FileUpload>> serviceProvider
        { new IFileUploadDeserializer with
            member __.Deserialize stream = instance.Deserialize stream
        })

  static let createDefaultUserSerializer =
    Func<IServiceProvider, _>
      (fun serviceProvider ->
        let instance = ActivatorUtilities.CreateInstance<DefaultSerializer<NCoreUtils.OAuth2.Rest.MappedUser>> serviceProvider
        { new IDefaultUserSerializer with
            member __.AsyncSerialize (output, item) = instance.AsyncSerialize (output, item)
        })

  static let createDefaultUserCollectionSerializer =
    Func<IServiceProvider, _>
      (fun serviceProvider ->
        let instance = ActivatorUtilities.CreateInstance<DefaultSerializer<NCoreUtils.OAuth2.Rest.MappedUser[]>> serviceProvider
        { new IDefaultUserCollectionSerializer with
            member __.AsyncSerialize (output, item) = instance.AsyncSerialize (output, item)
        })

  [<Extension>]
  static member AddCustomRestPipeline (services : IServiceCollection) =
    services
      .AddScoped<CurrentUploadData>()
      .AddScoped<IDefaultRestFileCreate>(createDefaultRestFileCreate)
      .AddScoped<IRestCreate<File, int>, RestFileCreate>()
      .AddScoped<IFileUploadDeserializer>(createFileUploadDeserializer)
      .AddScoped<IDeserializer<File>, FileDeserializer>()
      .AddScoped<IDefaultUserSerializer>(createDefaultUserSerializer)
      .AddScoped<ISerializer<User>, RestUserSerializer>()
      .AddScoped<IDefaultUserCollectionSerializer>(createDefaultUserCollectionSerializer)
      .AddScoped<ISerializer<User[]>, RestUserCollectionSerializer>()
