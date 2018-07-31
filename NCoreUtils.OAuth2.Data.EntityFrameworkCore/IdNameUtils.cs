using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;
using NCoreUtils.Linq;
using NCoreUtils.Text;

namespace NCoreUtils.OAuth2.Data
{
   static class IdNameUtils
    {
        sealed class Box<T>
        {
            public T Value;
        }

        static ConcurrentDictionary<Type, LambdaExpression> _idNameSelectors = new ConcurrentDictionary<Type, LambdaExpression>();

        static Expression BoxedConstant<T>(T value)
        {
            var box = new Box<T> { Value = value };
            return Expression.Field(Expression.Constant(box), nameof(Box<int>.Value));
        }

        static Expression<Func<TData, bool>> CreateIdNamePredicate<TData>(string value)
            where TData : IHasIdName
        {
            var idNameSelector = _idNameSelectors.GetOrAdd(
                key: typeof(TData),
                valueFactory: _ => LinqExtensions.ReplaceExplicitProperties<Func<TData, string>>(e => e.IdName));

            var eArg = Expression.Parameter(typeof(TData));
            return Expression.Lambda<Func<TData, bool>>(
                Expression.Equal(
                    idNameSelector.Body.SubstituteParameter(idNameSelector.Parameters[0], eArg),
                    BoxedConstant(value)
                ),
                eArg
            );
        }

        public static async Task<string> GenerateIdName<TData, TId>(
            this DataRepository<TData, TId> repository,
            ISimplifier simplifier,
            string source,
            Func<string, (string body, string suffix)> split,
            Func<string, int, string, string> rejoin,
            CancellationToken cancellationToken)
            where TData : class, IHasIdName, IHasId<TId>
            where TId : IComparable<TId>
        {
            var (name, suffix) = split(source);
            var baseName = simplifier.Simplify(name);
            var i = 0;
            string idName = null;
            while (null == idName)
            {
                var candidate = rejoin(baseName, i, suffix);
                if (await repository.Items.AnyAsync(CreateIdNamePredicate<TData>(candidate), cancellationToken))
                {
                    ++i;
                }
                else
                {
                    idName = candidate;
                }
            }
            return idName;
        }

        public static Task<string> GenerateIdName<TData, TId>(
            this DataRepository<TData, TId> repository,
            ISimplifier simplifier,
            string source,
            CancellationToken cancellationToken)
            where TData : class, IHasIdName, IHasId<TId>
            where TId : IComparable<TId>
            => GenerateIdName(
                repository: repository,
                simplifier: simplifier,
                source: source,
                split: name => (name, null),
                rejoin: (name, i, _) => i == 0 ? name : $"{name}-{i}",
                cancellationToken: cancellationToken);
    }
}