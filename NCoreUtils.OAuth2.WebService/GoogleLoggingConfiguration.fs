namespace NCoreUtils.OAuth2.WebService

open NCoreUtils.Logging

[<CLIMutable>]
type GoogleLoggingConfiguration = {
  ProjectId : string
  LogName   : string }
  with
    interface IGoogleLoggingConfiguration with
      member this.ProjectId = this.ProjectId
      member this.LogName   = this.LogName