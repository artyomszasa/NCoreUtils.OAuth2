namespace NCoreUtils.OAuth2

open Google.Apis.Auth.OAuth2
open Google.Apis.CloudKMS.v1
open NCoreUtils
open Google.Apis.CloudKMS.v1.Data
open System

type GoogleEncryptionProvider (configuration : GoogleEncryptionConfiguration) =

  static let googleCredentials =
    let credential = GoogleCredential.GetApplicationDefault();
    match credential.IsCreateScopedRequired with
    | true -> credential.CreateScoped [| Google.Apis.CloudKMS.v1.CloudKMSService.Scope.CloudPlatform |]
    | _    -> credential

  static let createKmsService () =
    new CloudKMSService (new Google.Apis.Services.BaseClientService.Initializer (HttpClientInitializer = googleCredentials, GZipEnabled = true))

  static let asyncDecrypt cryptoKey (cloudKms : CloudKMSService) decryptRequest =
    Async.Adapt (fun cancellationToken -> cloudKms.Projects.Locations.KeyRings.CryptoKeys.Decrypt(decryptRequest, cryptoKey).ExecuteAsync cancellationToken)

  static let asyncEncrypt cryptoKey (cloudKms : CloudKMSService) encryptRequest =
    Async.Adapt (fun cancellationToken -> cloudKms.Projects.Locations.KeyRings.CryptoKeys.Encrypt(encryptRequest, cryptoKey).ExecuteAsync cancellationToken)

  static let getPlaintext (response : DecryptResponse) = response.Plaintext

  static let getChiphertext (response : EncryptResponse) = response.Ciphertext

  let cryptoKey =
    sprintf "projects/%s/locations/%s/keyRings/%s/cryptoKeys/%s"
      configuration.ProjectId
      configuration.LocationId
      configuration.KeyRingId
      configuration.KeyId

  interface IEncryptionProvider with
    member __.Decrypt cipherData = async {
      use cloudKms = createKmsService ()
      return!
        DecryptRequest (Ciphertext = Convert.ToBase64String cipherData)
        |>  asyncDecrypt cryptoKey cloudKms
        >>| (getPlaintext >> Convert.FromBase64String) }
    member __.Encrypt plainData = async {
      use cloudKms = createKmsService ()
      return!
        EncryptRequest (Plaintext = Convert.ToBase64String plainData)
        |>  asyncEncrypt cryptoKey cloudKms
        >>| (getChiphertext >> Convert.FromBase64String) }




