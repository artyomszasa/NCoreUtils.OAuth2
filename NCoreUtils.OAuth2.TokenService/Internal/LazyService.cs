using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.OAuth2.Internal;

public class LazyService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider serviceProvider)
    where T : notnull
{
    private readonly Lazy<T> _instance = new(serviceProvider.GetRequiredService<T>);

    public T Instance => _instance.Value;
}