namespace NCoreUtils.OAuth2

open System
open Google.Apis.Auth.OAuth2
open Google.Apis.CloudKMS.v1
open Google.Apis.CloudKMS.v1.Data
open NCoreUtils
open System.Collections.Concurrent

type private CloudKMSServiceEntry = {
  Instance : CloudKMSService
  Count    : int }

type private CloudKMSServicePool () =
  static let googleCredentials =
    let credential = GoogleCredential.GetApplicationDefault();
    match credential.IsCreateScopedRequired with
    | true -> credential.CreateScoped [| Google.Apis.CloudKMS.v1.CloudKMSService.Scope.CloudPlatform |]
    | _    -> credential

  let queue = ConcurrentQueue ()

  member __.Count = queue.Count

  member __.Take () =
    let mutable entry = Unchecked.defaultof<_>
    match queue.TryDequeue (&entry) with
    | true -> entry
    | _    ->
      { Instance = new CloudKMSService (new Google.Apis.Services.BaseClientService.Initializer (HttpClientInitializer = googleCredentials, GZipEnabled = true))
        Count    = 0 }

  member __.Return (entry : CloudKMSServiceEntry) =
    match entry.Count >= 128 with
    | true ->
      entry.Instance.Dispose ()
    | _ -> queue.Enqueue { entry with Count = entry.Count + 1 }

[<AutoOpen>]
module private GoogleEncryptionProviderHelpers =

  let inline asyncDecrypt cryptoKey (cloudKms : CloudKMSService) decryptRequest =
    Async.Adapt (fun cancellationToken -> cloudKms.Projects.Locations.KeyRings.CryptoKeys.Decrypt(decryptRequest, cryptoKey).ExecuteAsync cancellationToken)

  let inline asyncEncrypt cryptoKey (cloudKms : CloudKMSService) encryptRequest =
    Async.Adapt (fun cancellationToken -> cloudKms.Projects.Locations.KeyRings.CryptoKeys.Encrypt(encryptRequest, cryptoKey).ExecuteAsync cancellationToken)

  let inline getPlaintext (response : DecryptResponse) = response.Plaintext

  let inline getChiphertext (response : EncryptResponse) = response.Ciphertext

  let inline getPlaintextAndConvert (response : DecryptResponse) =
    getPlaintext response
    |> Convert.FromBase64String

  let inline getChiphertextAndConvert (response : EncryptResponse) =
    getChiphertext response
    |> Convert.FromBase64String

type GoogleEncryptionProvider (configuration : GoogleEncryptionConfiguration) =

  static let pool = new CloudKMSServicePool ()

  // argument check
  do
    if isNull (box configuration) then ArgumentNullException "configuration" |> raise
    if String.IsNullOrWhiteSpace configuration.ProjectId then
      invalidArg "configuration.ProjectId" "ProjectId must be a non-empty string."
    if String.IsNullOrWhiteSpace configuration.LocationId then
      invalidArg "configuration.LocationId" "LocationId must be a non-empty string."
    if String.IsNullOrWhiteSpace configuration.KeyRingId then
      invalidArg "configuration.KeyRingId" "KeyRingId must be a non-empty string."
    if String.IsNullOrWhiteSpace configuration.KeyId then
      invalidArg "configuration.KeyId" "KeyId must be a non-empty string."

  let cryptoKey =
    sprintf "projects/%s/locations/%s/keyRings/%s/cryptoKeys/%s"
      configuration.ProjectId
      configuration.LocationId
      configuration.KeyRingId
      configuration.KeyId

  interface IEncryptionProvider with
    member __.Decrypt cipherData = async {
      let cloudKmsEntry = pool.Take ()
      try
        let! response =
          DecryptRequest (Ciphertext = Convert.ToBase64String cipherData)
          |> asyncDecrypt cryptoKey cloudKmsEntry.Instance
        return getPlaintextAndConvert response
      finally
        pool.Return cloudKmsEntry }
    member __.Encrypt plainData = async {
      let cloudKms = pool.Take ()
      try
        let! response =
          EncryptRequest (Plaintext = Convert.ToBase64String plainData)
          |> asyncEncrypt cryptoKey cloudKms.Instance
        return getChiphertextAndConvert response
      finally
        pool.Return cloudKms }




