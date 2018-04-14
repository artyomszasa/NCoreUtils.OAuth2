namespace NCoreUtils.OAuth2

[<RequireQualifiedAccess>]
module Permissions =

  [<RequireQualifiedAccess>]
  module User =

    [<Literal>]
    let Write = "user.write"

    [<Literal>]
    let Read = "user.read"

    [<Literal>]
    let Delete = "user.delete"

  [<RequireQualifiedAccess>]
  module Permission =

    [<Literal>]
    let Write = "permission.write"

    [<Literal>]
    let Read = "permission.read"


    [<Literal>]
    let Delete = "user.delete"