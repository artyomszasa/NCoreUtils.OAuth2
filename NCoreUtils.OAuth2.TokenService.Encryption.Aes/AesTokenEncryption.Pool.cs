using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace NCoreUtils.OAuth2
{
    public partial class AesTokenEncryption
    {
        private sealed class EncryptorPool
        {
            private readonly ConcurrentQueue<ICryptoTransform> _queue = new ConcurrentQueue<ICryptoTransform>();

            private readonly Aes _alg;

            public EncryptorPool(Aes alg)
            {
                _alg = alg ?? throw new ArgumentNullException(nameof(alg));
            }

            public ICryptoTransform Rent()
                => _queue.TryDequeue(out var instance) ? instance : _alg.CreateEncryptor();

            public void Return(ICryptoTransform instance)
            {
                if (instance.CanReuseTransform)
                {
                    _queue.Enqueue(instance);
                }
                else
                {
                    instance.Dispose();
                }
            }
        }

        private sealed class DecryptorPool
        {
            private readonly ConcurrentQueue<ICryptoTransform> _queue = new ConcurrentQueue<ICryptoTransform>();

            private readonly Aes _alg;

            public DecryptorPool(Aes alg)
            {
                _alg = alg ?? throw new ArgumentNullException(nameof(alg));
            }

            public ICryptoTransform Rent()
                => _queue.TryDequeue(out var instance) ? instance : _alg.CreateDecryptor();

            public void Return(ICryptoTransform instance)
            {
                if (instance.CanReuseTransform)
                {
                    _queue.Enqueue(instance);
                }
                else
                {
                    instance.Dispose();
                }
            }
        }
    }
}