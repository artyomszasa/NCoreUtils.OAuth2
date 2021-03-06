namespace NCoreUtils.OAuth2.Data

open System
open System.Diagnostics.CodeAnalysis
open System.IO
open System.Runtime.CompilerServices
open NCoreUtils

[<CustomEquality; NoComparison>]
type Token = {
  Id        : string
  IssuedAt  : DateTimeOffset
  ExpiresAt : DateTimeOffset
  Scopes    : Set<CaseInsensitive> }
  with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member internal Eq (a : Token, b : Token) =
      match obj.ReferenceEquals (a, null) with
      | true -> obj.ReferenceEquals (b, null)
      | _    ->
      match obj.ReferenceEquals (b, null) with
      | true -> false
      | _    ->
        StringComparer.OrdinalIgnoreCase.Equals (a.Id, b.Id)
          && a.IssuedAt = b.IssuedAt
          && a.ExpiresAt = b.ExpiresAt
          && a.Scopes = b.Scopes

    [<ExcludeFromCodeCoverage>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member op_Equality (a, b) = Token.Eq (a, b)

    [<ExcludeFromCodeCoverage>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member op_Inequality (a, b) = not (Token.Eq (a, b))

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Create (id : string, issuedAt : DateTimeOffset, expiresAt : DateTimeOffset, scopes : seq<CaseInsensitive>) =
      { Id = id; IssuedAt = issuedAt; ExpiresAt = expiresAt; Scopes = Set.ofSeq scopes }

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Create (id : string, issuedAt : DateTimeOffset, expiresAt : DateTimeOffset, scopes : seq<string>) =
      Token.Create (id, issuedAt, expiresAt, Seq.map CaseInsensitive scopes)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Equals other = Token.Eq (this, other)

    interface IEquatable<Token> with
      member this.Equals other = Token.Eq (this, other)

    override this.Equals obj =
      match obj with
      | :? Token as other -> Token.Eq (this, other)
      | _                 -> false

    override this.GetHashCode () =
      let mutable hash = 17;
      hash <- hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Id);
      hash <- hash * 23 + this.IssuedAt.GetHashCode();
      hash <- hash * 23 + this.ExpiresAt.GetHashCode();
      for scope in this.Scopes do
        hash <- hash * 23 + scope.GetHashCode ();
      hash

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Token =
  [<Literal>]
  let private Magic = 0x7EE7us

  [<ExcludeFromCodeCoverage>]
  let inline id ({ Id = id } : Token) = id

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let expiresIn ({ IssuedAt = issuedAt; ExpiresAt = expiresAt } : Token) =
    (expiresAt - issuedAt).TotalSeconds
    |> round
    |> int

  [<CompiledName("ReadFrom")>]
  let readFrom (reader : BinaryReader) =
    try
      let magic = reader.ReadUInt16 ()
      if Magic <> magic then
        FormatException "Invalid magic bytes in token." |> raise
      let id             = reader.ReadString ()
      let issuedAtTicks  = reader.ReadInt64();
      let expiresAtTicks = reader.ReadInt64();
      let scopeCount     = reader.ReadInt32();
      let scopes =
        Seq.init scopeCount (ignore >> reader.ReadString)
        |> Seq.map CaseInsensitive
        |> Set.ofSeq
      { Id        = id
        IssuedAt  = DateTimeOffset (issuedAtTicks, TimeSpan.Zero)
        ExpiresAt = DateTimeOffset (expiresAtTicks, TimeSpan.Zero)
        Scopes    = scopes }
    with
      | :? EndOfStreamException -> FormatException "Invalid token length." |> raise
      | e -> reraise ()

  [<CompiledName("WriteTo")>]
  let writeTo (writer : BinaryWriter) ({ Id = id; IssuedAt = issuedAt; ExpiresAt = expiresAt; Scopes = scopes } : Token) =
    writer.Write Magic
    writer.Write id
    writer.Write issuedAt.UtcTicks
    writer.Write expiresAt.UtcTicks
    writer.Write scopes.Count
    scopes |> Set.iter (fun scope -> writer.Write scope.Value)

