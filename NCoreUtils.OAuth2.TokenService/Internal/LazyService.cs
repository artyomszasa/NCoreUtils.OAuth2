using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.OAuth2.Internal
{
    public class LazyService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
        where T : notnull
    {
        private readonly Lazy<T> _instance;

        public T Instance => _instance.Value;

        public LazyService(IServiceProvider serviceProvider)
        {
            _instance = new Lazy<T>(() => serviceProvider.GetRequiredService<T>());
        }
    }
}