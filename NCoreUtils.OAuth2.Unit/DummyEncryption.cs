using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;

namespace NCoreUtils.OAuth2.Unit
{
  public class DummyEncryption : AsyncEncryptionProvider
  {
    public override Task<byte[]> DecryptAsync(byte[] cipherData, CancellationToken cancellationToken)
    {
      return Task.FromResult(cipherData);
    }

    public override Task<byte[]> EncryptAsync(byte[] plainData, CancellationToken cancellationToken)
    {
      return Task.FromResult(plainData);
    }
  }
}