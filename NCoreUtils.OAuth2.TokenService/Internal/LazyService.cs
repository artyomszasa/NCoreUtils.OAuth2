using System;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.OAuth2.Internal
{
    public class LazyService<T>
    {
        private readonly Lazy<T> _instance;

        public T Instance => _instance.Value;

        public LazyService(IServiceProvider serviceProvider)
        {
            _instance = new Lazy<T>(() => serviceProvider.GetRequiredService<T>());
        }
    }
}