using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NCoreUtils.Memory;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;
/// <summary>
/// Scope collection that distinguishes between no value and empty value.
/// </summary>
[JsonConverter(typeof(ScopeCollectionConverter))]
public partial struct ScopeCollection
    : IReadOnlyCollection<string>
    , IEquatable<ScopeCollection>
    , ISpanExactEmplaceable
// NOTE: compatibility / should be removed in uture releases
#pragma warning disable CS0618
    , IEmplaceable<ScopeCollection>
#pragma warning restore CS0618
#if NET7_0_OR_GREATER
    , IParsable<ScopeCollection>
#else
    , IFormattable
#endif
{
    [ExcludeFromCodeCoverage]
    private sealed class EmptyEnumerator : IEnumerator<string>
    {
        public static IEnumerator<string> Instance { get; } = new EmptyEnumerator();

        object IEnumerator.Current => default!;

        public string Current => default!;

        private EmptyEnumerator() { }

        public void Dispose() { }

        public bool MoveNext() => false;

        public void Reset() { }
    }

    private const int MaxCharBufferStackAllocSize = 8 * 1024;

    private const int MaxCharBufferPoolAllocSize = 32 * 1024;

    private static readonly IEqualityComparer<HashSet<string>> _equalityComparer = HashSet<string>.CreateSetComparer();

    private static readonly Regex _regexWs = new("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static bool operator==(ScopeCollection a, ScopeCollection b)
        => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static bool operator!=(ScopeCollection a, ScopeCollection b)
        => !a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static implicit operator ScopeCollection(List<string>? scopes)
        => new(scopes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static implicit operator ScopeCollection(string[]? scopes)
        => new(scopes!);

    private static int GetEmplaceBufferSize(HashSet<string> scopes)
    {
        var first = true;
        var result = 0;
        foreach (var scope in scopes)
        {
            result += scope.Length;
            if (first)
            {
                first = false;
            }
            else
            {
                ++result;
            }
        }
        return result;
    }

    public static ScopeCollection Parse(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return default;
        }
        return new ScopeCollection(_regexWs.Split(input));
    }

    public static ScopeCollection Parse(string? input, IFormatProvider? formatProvider)
        => Parse(input);

    public static bool TryParse(string? input, IFormatProvider? formatProvider, out ScopeCollection result)
    {
        result = Parse(input);
        return true;
    }

    internal readonly HashSet<string>? _scopes;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => _scopes is null ? 0 : _scopes.Count;
    }

    /// <summary>
    /// Gets whether collection ha no value set.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_scopes))]
    public bool HasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => _scopes is not null;
    }

    /// <summary>
    /// Gets whether collection is empty (either no value or empty value).
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => Count == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public ScopeCollection(IEnumerable<string>? scopes)
    {
        if (scopes is null)
        {
            _scopes = default;
        }
        else
        {
            // in order to minimize allocations check whether scope set size is known...
            var initialCapacity = scopes switch
            {
                IReadOnlyCollection<string> ro => ro.Count,
                ICollection<string> rw => rw.Count,
                _ => 4
            };
            var scopeSet = new HashSet<string>(initialCapacity);
            foreach (var scope in scopes)
            {
                if (!string.IsNullOrEmpty(scope))
                {
                    scopeSet.Add(scope);
                }
            }
            _scopes = scopeSet;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public ScopeCollection(params string[]? scopes)
    {
        if (scopes is null)
        {
            _scopes = default;
        }
        else
        {
            var scopeSet = new HashSet<string>(scopes.Length);
            foreach (var scope in scopes)
            {
                if (!string.IsNullOrEmpty(scope))
                {
                    scopeSet.Add(scope);
                }
            }
            _scopes = scopeSet;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    int ISpanExactEmplaceable.GetEmplaceBufferSize() => GetEmplaceBufferSize();

    private int GetEmplaceBufferSize()
    {
        if (_scopes is null || _scopes.Count == 0)
        {
            return 1;
        }
        return GetEmplaceBufferSize(_scopes);
    }

    string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider)
        => ToString();

    public int ComputeRequiredBufferSize()
    {
        if (_scopes is null || _scopes.Count == 0)
        {
            return 0;
        }
        return GetEmplaceBufferSize(_scopes);
    }

    public int Emplace(System.Span<char> span)
    {
        if (TryEmplace(span, out var used))
        {
            return used;
        }
        throw new InsufficientBufferSizeException(span, ComputeRequiredBufferSize());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ScopeCollection other)
    {
        if (_scopes is null)
        {
            return other._scopes is null;
        }
        if (other._scopes is null)
        {
            return false;
        }
        return ReferenceEquals(_scopes, other._scopes) || _equalityComparer.Equals(_scopes, other._scopes);
    }

    public override bool Equals(object? obj)
        => obj is ScopeCollection other && Equals(other);

    public IEnumerator<string> GetEnumerator()
        => _scopes is null ? EmptyEnumerator.Instance : _scopes.GetEnumerator();

    public override int GetHashCode()
        => _scopes is null ? 0 : _equalityComparer.GetHashCode(_scopes);

    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }
        return this.ToStringUsingArrayPool();
    }

    public bool TryEmplace(Span<char> span, out int used)
    {
        if (_scopes is null || _scopes.Count == 0)
        {
            used = 0;
            return true;
        }
        var builder = new SpanBuilder(span);
        var first = true;
        foreach (var scope in _scopes)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                if (!builder.TryAppend(' ')) { used = 0; return false; }
            }
            if (!builder.TryAppend(scope)) { used = 0; return false; }
        }
        used = builder.Length;
        return true;
    }
}