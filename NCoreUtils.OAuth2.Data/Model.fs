namespace NCoreUtils.OAuth2.Data

open System
open System.Collections.Generic
open NCoreUtils.Data
open NCoreUtils.Authentication

type [<CLIMutable>] ClientApplication = {
  mutable Id          : int
  mutable Name        : string
  mutable Description : string
  mutable Domains     : ICollection<Domain>
  mutable Users       : ICollection<User>
  mutable Permissions : ICollection<Permission> }
  with
    interface IHasId<int> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id

and [<CLIMutable>] Domain = {
  mutable Id                  : int
  mutable DomainName          : string
  mutable ClientApplicationId : int
  mutable ClientApplication   : ClientApplication }
  with
    interface IHasId<int> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id

and [<CLIMutable>] Permission = {
  mutable Id                  : int
  mutable Name                : string
  mutable Description         : string
  mutable ClientApplicationId : int
  mutable ClientApplication   : ClientApplication
  mutable Users               : ICollection<UserPermission> }
  with
    interface IHasId<int> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id

and [<CLIMutable>] User = {
  mutable Id                  : int
  mutable Created             : int64
  mutable Updated             : int64
  mutable State               : State
  mutable ClientApplicationId : int
  mutable ClientApplication   : ClientApplication
  mutable HonorificPrefix     : string
  mutable FamilyName          : string
  mutable GivenName           : string
  mutable MiddleName          : string
  mutable Email               : string
  mutable Salt                : string
  mutable Password            : string
  mutable Permissions         : ICollection<UserPermission>
  mutable RefreshTokens       : ICollection<RefreshToken>
  mutable AuthorizationCodes  : ICollection<AuthorizationCode> }
  with
    interface IHasId<int> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id
    interface IUser with
      [<TargetProperty("Email")>]
      member this.Email = this.Email
      [<TargetProperty("FamilyName")>]
      member this.FamilyName = this.FamilyName
      [<TargetProperty("GivenName")>]
      member this.GivenName = this.GivenName
    interface IUser<int> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id
    interface ILocalUser<int> with
      [<TargetProperty("Password")>]
      member this.Password = this.Password
      [<TargetProperty("Salt")>]
      member this.Salt = this.Salt
    interface IHasState with
      [<TargetProperty("State")>]
      member this.State
        with get ()    = this.State
        and  set value = this.State <- value
    interface IHasTimeTracking with
      [<TargetProperty("Created")>]
      member this.Created
        with get ()    = this.Created
        and  set value = this.Created <- value
      [<TargetProperty("Updated")>]
      member this.Updated
        with get ()    = this.Updated
        and  set value = this.Updated <- value

and [<CLIMutable>] UserPermission = {
  mutable UserId       : int
  mutable User         : User
  mutable PermissionId : int
  mutable Permission   : Permission }

and [<CLIMutable>] RefreshToken = {
  mutable Id        : int64
  mutable State     : State
  mutable UserId    : int
  mutable User      : User
  mutable IssuedAt  : int64
  mutable ExpiresAt : int64
  mutable Scopes    : string
  mutable LastUsed  : Nullable<int64> }
  with
    interface IHasId<int64> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id


and [<CLIMutable>] AuthorizationCode = {
  mutable Id          : Guid
  mutable UserId      : int
  mutable User        : User
  mutable IssuedAt    : int64
  mutable ExpiresAt   : int64
  mutable RedirectUri : string
  mutable Scopes      : string }
  with
    interface IHasId<Guid> with
      [<TargetProperty("Id")>]
      member this.Id = this.Id

