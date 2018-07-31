using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;
using NCoreUtils.Text;

namespace NCoreUtils.OAuth2.Data
{
    class FileRepository : DataRepository<File, int>
    {
        static readonly Random _random = new Random();

        static readonly Regex _ext = new Regex("^(.*)\\.([^.]+)$");

        static readonly string[] _alpha;

        static FileRepository()
        {
            _alpha = new string[256];
            for (var i = 0; i < 256; ++i)
            {
                _alpha[i] = i.ToString("x2");
            }
        }

        static (string name, string suffix) SplitName(string name)
        {
            var m = _ext.Match(name);
            if (m.Success)
            {
                return (m.Groups[1].Value, m.Groups[2].Value.ToLowerInvariant());
            }
            return (name, null);
        }

        readonly ISimplifier _simplifier;

        public FileRepository(IServiceProvider serviceProvider, DataRepositoryContext context, ISimplifier simplifier, IDataEventHandlers eventHandlers = null)
            : base(serviceProvider, context, eventHandlers)
        {
            _simplifier = simplifier;
        }

        public override async Task<File> PersistAsync(File item, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!item.HasValidId() || string.IsNullOrEmpty(item.IdName))
            {
                var prefix = _alpha[_random.Next() % 256];
                var idName = await IdNameUtils.GenerateIdName(
                    repository: this,
                    simplifier: _simplifier,
                    source: item.OriginalName,
                    split: SplitName,
                    rejoin: (name, i, suffix) => null == suffix ? (0 == i ? name : $"{name}.{suffix}") : (0 == i ? $"{name}.{suffix}" : $"{name}-{i}.{suffix}"),
                    cancellationToken: cancellationToken);
                item.IdName = $"{prefix}/{idName}";
            }
            return await base.PersistAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }
}