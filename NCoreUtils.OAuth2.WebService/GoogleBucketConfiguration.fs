namespace NCoreUtils.OAuth2

[<AllowNullLiteral>]
type GoogleBucketConfiguration () =
  member val ProjectId  = Unchecked.defaultof<string> with get, set
  member val BucketName = Unchecked.defaultof<string> with get, set
  override this.ToString () = sprintf "GoogleBucketConfiguration[PorjectId = %s, BucketName = %s]" this.ProjectId this.BucketName