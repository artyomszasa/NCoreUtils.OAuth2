namespace NCoreUtils.OAuth2.Data

open System
open System.Collections.Generic
open NCoreUtils.Data
open NCoreUtils.Authentication
open Newtonsoft.Json
open NCoreUtils.AspNetCore

[<AutoOpen>]
module private DateTimeHelpers =
  let inline fmtAsDateTime (utcTicks : int64) =
    DateTimeOffset(utcTicks, TimeSpan.Zero).ToLocalTime().ToString("o")

type IHasIdName =
  abstract IdName : string with get, set

type File () =
  member val Id           = Unchecked.defaultof<int>    with get, set
  member val Created      = Unchecked.defaultof<int64>  with get, set
  member val Updated      = Unchecked.defaultof<int64>  with get, set
  member val IdName       = Unchecked.defaultof<string> with get, set
  member val OriginalName = Unchecked.defaultof<string> with get, set
  member val MediaType    = Unchecked.defaultof<string> with get, set
  member val State        = Unchecked.defaultof<State>  with get, set
  interface IHasId<int> with
    member this.Id with [<TargetProperty("Id")>] get () = this.Id
  interface IFile with
    member this.IdName       = this.IdName
    member this.OriginalName = this.OriginalName
    member this.MediaType    = this.MediaType
  interface IHasIdName with
    member this.IdName with [<TargetProperty("IdName")>] get () = this.IdName
    member this.IdName with [<TargetProperty("IdName")>] set value = this.IdName <- value
  override this.ToString () =
    sprintf "File[Id = %d, Created = %s, Updated = %s, IdName = %s, OriginalName = %s, MediaType = %s, State = %A]"
      this.Id
      (fmtAsDateTime this.Created)
      (fmtAsDateTime this.Updated)
      this.IdName
      this.OriginalName
      this.MediaType
      this.State

type [<CLIMutable; NoEquality; NoComparison>] ClientApplication = {
  [<field: JsonIgnore>]
  mutable Id          : int
  [<field: JsonIgnore>]
  mutable Name        : string
  [<field: JsonIgnore>]
  mutable Description : string
  [<field: JsonIgnore>]
  mutable Domains     : ICollection<Domain>
  [<field: JsonIgnore>]
  mutable Users       : ICollection<User>
  [<field: JsonIgnore>]
  mutable Permissions : ICollection<Permission> }
  with
    interface IHasId<int> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id

and [<CLIMutable; NoEquality; NoComparison>] Domain = {
  [<field: JsonIgnore>]
  mutable Id                  : int
  [<field: JsonIgnore>]
  mutable DomainName          : string
  [<field: JsonIgnore>]
  mutable ClientApplicationId : int
  [<field: JsonIgnore>]
  mutable ClientApplication   : ClientApplication }
  with
    interface IHasId<int> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id

and [<CLIMutable; NoEquality; NoComparison>] Permission = {
  [<field: JsonIgnore>]
  mutable Id                  : int
  [<field: JsonIgnore>]
  mutable Name                : string
  [<field: JsonIgnore>]
  mutable Description         : string
  [<field: JsonIgnore>]
  mutable ClientApplicationId : int
  [<field: JsonIgnore>]
  mutable ClientApplication   : ClientApplication
  [<field: JsonIgnore>]
  mutable Users               : ICollection<UserPermission> }
  with
    interface IHasId<int> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id

and [<CLIMutable; NoEquality; NoComparison>] User = {
  [<field: JsonIgnore>]
  mutable Id                  : int
  [<field: JsonIgnore>]
  mutable Created             : int64
  [<field: JsonIgnore>]
  mutable Updated             : int64
  [<field: JsonIgnore>]
  mutable State               : State
  [<field: JsonIgnore>]
  mutable ClientApplicationId : int
  [<field: JsonIgnore>]
  mutable ClientApplication   : ClientApplication
  [<field: JsonIgnore>]
  mutable HonorificPrefix     : string
  [<field: JsonIgnore>]
  mutable FamilyName          : string
  [<field: JsonIgnore>]
  mutable GivenName           : string
  [<field: JsonIgnore>]
  mutable MiddleName          : string
  [<field: JsonIgnore>]
  mutable Email               : string
  [<field: JsonIgnore>]
  mutable Salt                : string
  [<field: JsonIgnore>]
  mutable Password            : string
  [<field: JsonIgnore>]
  mutable AvatarId            : Nullable<int>
  [<field: JsonIgnore>]
  mutable Avatar              : File
  [<field: JsonIgnore>]
  mutable Permissions         : ICollection<UserPermission>
  [<field: JsonIgnore>]
  mutable RefreshTokens       : ICollection<RefreshToken>
  [<field: JsonIgnore>]
  mutable AuthorizationCodes  : ICollection<AuthorizationCode> }
  with
    interface IHasId<int> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id
    interface IUser with
      member this.Email with [<TargetProperty("Email")>] get () = this.Email
      member this.FamilyName with [<TargetProperty("FamilyName")>] get () = this.FamilyName
      member this.GivenName with [<TargetProperty("GivenName")>] get () = this.GivenName
    interface IUser<int> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id
    interface ILocalUser<int> with
      member this.Password with [<TargetProperty("Password")>] get () = this.Password
      member this.Salt with [<TargetProperty("Salt")>] get () = this.Salt
    interface IHasState with
      member this.State
        with [<TargetProperty("State")>] get ()    = this.State
        and  [<TargetProperty("State")>] set value = this.State <- value
    interface IHasTimeTracking with
      member this.Created
        with [<TargetProperty("Created")>] get ()    = this.Created
        and  [<TargetProperty("Created")>] set value = this.Created <- value
      member this.Updated
        with [<TargetProperty("Updated")>] get ()    = this.Updated
        and  [<TargetProperty("Updated")>] set value = this.Updated <- value

and [<CLIMutable; NoEquality; NoComparison>] UserPermission = {
  [<field: JsonIgnore>]
  mutable UserId       : int
  [<field: JsonIgnore>]
  mutable User         : User
  [<field: JsonIgnore>]
  mutable PermissionId : int
  [<field: JsonIgnore>]
  mutable Permission   : Permission }

and [<CLIMutable; NoEquality; NoComparison>] RefreshToken = {
  [<field: JsonIgnore>]
  mutable Id        : int64
  [<field: JsonIgnore>]
  mutable State     : State
  [<field: JsonIgnore>]
  mutable UserId    : int
  [<field: JsonIgnore>]
  mutable User      : User
  [<field: JsonIgnore>]
  mutable IssuedAt  : int64
  [<field: JsonIgnore>]
  mutable ExpiresAt : int64
  [<field: JsonIgnore>]
  mutable Scopes    : string
  [<field: JsonIgnore>]
  mutable LastUsed  : Nullable<int64> }
  with
    interface IHasId<int64> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id


and [<CLIMutable; NoEquality; NoComparison>] AuthorizationCode = {
  [<field: JsonIgnore>]
  mutable Id          : Guid
  [<field: JsonIgnore>]
  mutable UserId      : int
  [<field: JsonIgnore>]
  mutable User        : User
  [<field: JsonIgnore>]
  mutable IssuedAt    : int64
  [<field: JsonIgnore>]
  mutable ExpiresAt   : int64
  [<field: JsonIgnore>]
  mutable RedirectUri : string
  [<field: JsonIgnore>]
  mutable Scopes      : string }
  with
    interface IHasId<Guid> with
      member this.Id with [<TargetProperty("Id")>] get () = this.Id

