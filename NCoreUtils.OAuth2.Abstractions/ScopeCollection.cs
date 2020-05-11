using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NCoreUtils.Memory;

namespace NCoreUtils.OAuth2
{
    /// <summary>
    /// Scope collection that distinguishes between no value and empty value.
    /// </summary>
    public struct ScopeCollection : IReadOnlyCollection<string>, IEquatable<ScopeCollection>, IEmplaceable<ScopeCollection>
    {
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

        private static readonly IEqualityComparer<HashSet<string>> _equalityComparer = HashSet<string>.CreateSetComparer();

        private static readonly Regex _regexWs = new Regex("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThroughAttribute]
        public static bool operator==(ScopeCollection a, ScopeCollection b)
            => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThroughAttribute]
        public static bool operator!=(ScopeCollection a, ScopeCollection b)
            => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThroughAttribute]
        public static implicit operator ScopeCollection(List<string>? scopes)
            => new ScopeCollection(scopes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThroughAttribute]
        public static implicit operator ScopeCollection(string[]? scopes)
            => new ScopeCollection(scopes);

        public static ScopeCollection Parse(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return default;
            }
            return new ScopeCollection(_regexWs.Split(input));
        }

        private readonly HashSet<string>? _scopes;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerStepThroughAttribute]
            get => _scopes is null ? 0 : _scopes.Count;
        }

        /// <summary>
        /// Gets whether collection ha no value set.
        /// </summary>
        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerStepThroughAttribute]
            get => _scopes != null;
        }

        /// <summary>
        /// Gets whether collection is empty (either no value or empty value).
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerStepThroughAttribute]
            get => Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThroughAttribute]
        public ScopeCollection(IEnumerable<string>? scopes)
            // => _scopes = scopes is null ? default : new HashSet<string>(scopes);
        {
            if (scopes is null)
            {
                _scopes = default;
            }
            else
            {
                var scopeSet = new HashSet<string>();
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
            => string.Join(" ", this);

        public int Emplace(Span<char> span)
        {
            if (_scopes is null)
            {
                return 0;
            }
            var first = true;
            var builder = new SpanBuilder(span);
            foreach (var scope in _scopes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(' ');
                }
                builder.Append(scope);
            }
            return builder.Length;
        }

        public bool TryEmplace(Span<char> span, out int used)
        {
            if (_scopes is null)
            {
                used = 0;
                return true;
            }
            var first = true;
            var builder = new SpanBuilder(span);
            foreach (var scope in _scopes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(' '))
                    {
                        used = default;
                        return false;
                    }
                }
                if (!builder.TryAppend(scope))
                {
                    used = default;
                    return false;
                }
            }
            used = builder.Length;
            return true;
        }
    }
}