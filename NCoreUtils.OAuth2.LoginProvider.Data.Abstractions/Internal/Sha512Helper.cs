using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NCoreUtils.OAuth2.Internal;
public static class Sha512Helper
{
    private static readonly ConcurrentQueue<SHA512> _queue = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SHA512 Rent()
        => _queue.TryDequeue(out var instance) ? instance : SHA512.Create();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(SHA512 instance)
        => _queue.Enqueue(instance);
}