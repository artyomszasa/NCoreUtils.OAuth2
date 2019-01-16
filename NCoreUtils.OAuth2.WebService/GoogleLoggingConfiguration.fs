namespace NCoreUtils.OAuth2.WebService

open NCoreUtils.Logging.Google

[<CLIMutable>]
type GoogleLoggingConfiguration = {
  EnvNodeName    : string
  EnvPodName     : string
  ProjectId      : string
  ServiceName    : string
  ServiceVersion : string }
  with
    interface IGoogleAspNetCoreLoggingConfiguration with
      member this.EnvNodeName    = this.EnvNodeName
      member this.EnvPodName     = this.EnvPodName
      member this.ProjectId      = this.ProjectId
      member this.ServiceName    = this.ServiceName
      member this.ServiceVersion = this.ServiceVersion
      member __.PopulateLabels (_, _, _, _, _, _) = ()