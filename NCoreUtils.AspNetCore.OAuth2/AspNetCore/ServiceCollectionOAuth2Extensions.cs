using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Proto;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace NCoreUtils.AspNetCore
{
    public static class ServiceCollectionOAuth2Extensions
    {
        private static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            ServiceDescriptor tokenHandler,
            TokenServiceEndpointsClientConfiguration configuration)
        {
            services
                .AddTokenServiceClient(configuration)
                .Add(tokenHandler);
            services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                .AddRemoteOAuth2AuthenticationScheme();
            return services;
        }

        private static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            ServiceDescriptor tokenHandler,
            TokenServiceEndpointsClientConfiguration configuration,
            IntrospectionCacheOptions cacheOptions)
        {
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            services
                .AddTokenServiceClient(configuration)
                .Add(tokenHandler);
            services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                .AddRemoteOAuth2AuthenticationScheme();
            return services;
        }

        // private static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
        //     this IServiceCollection services,
        //     ServiceDescriptor tokenHandler,
        //     IEndpointConfiguration configuration)
        //     where TAuthenticationHandler : OAuth2AuthenticationHandler
        // {
        //     services
        //         .AddTokenServiceClient(configuration)
        //         .Add(tokenHandler);
        //     services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
        //         .AddCustomRemoteOAuth2AuthenticationScheme<TAuthenticationHandler>();
        //     return services;
        // }

        private static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            ServiceDescriptor tokenHandler,
            TokenServiceEndpointsClientConfiguration configuration,
            IntrospectionCacheOptions cacheOptions)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            services
                .AddTokenServiceClient(configuration)
                .Add(tokenHandler);
            services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                .AddCustomRemoteOAuth2AuthenticationScheme<TAuthenticationHandler>();
            return services;
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTokenHandler>(
            this IServiceCollection services,
            TokenServiceEndpointsClientConfiguration configuration,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Scoped)
            where TTokenHandler : class, ITokenHandler
            => services.AddRemoteOAuth2Authentication(
                ServiceDescriptor.Describe(typeof(ITokenHandler), typeof(TTokenHandler), tokenHandlerLifetime),
                configuration
            );

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            TokenServiceEndpointsClientConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var handlers = new List<Func<IServiceProvider, ITokenHandler>>(4);
            if (tokenHandlers.HasFlag(TokenHandlers.Bearer))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<BearerTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Cookie))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<CookieTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Query))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<QueryTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Form))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<FormTokenHandler>(serviceProvider));
            }
            Func<IServiceProvider, ITokenHandler> factory = handlers.Count == 1
                ? handlers[0]
                : (serviceProvider => new CompositeTokenHandler(handlers.Select(f => f(serviceProvider)).ToList()));
            return services.AddRemoteOAuth2Authentication(
                ServiceDescriptor.Scoped(factory),
                configuration,
                cacheOptions
            );
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            TokenServiceEndpointsClientConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var handlers = new List<Func<IServiceProvider, ITokenHandler>>(4);
            if (tokenHandlers.HasFlag(TokenHandlers.Bearer))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<BearerTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Cookie))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<CookieTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Query))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<QueryTokenHandler>(serviceProvider));
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Form))
            {
                handlers.Add(serviceProvider => ActivatorUtilities.CreateInstance<FormTokenHandler>(serviceProvider));
            }
            Func<IServiceProvider, ITokenHandler> factory = handlers.Count == 1
                ? handlers[0]
                : (serviceProvider => new CompositeTokenHandler(handlers.Select(f => f(serviceProvider)).ToList()));
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
                ServiceDescriptor.Scoped(factory),
                configuration,
                cacheOptions
            );
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTokenHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Scoped)
            where TTokenHandler : class, ITokenHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration
            {
                Endpoint = configuration["Endpoint"] ?? throw new InvalidOperationException("No endpoint for TokenService"),
                HttpClient = configuration["HttpClient"],
                Path = configuration["Path"]
            };
            return services.AddRemoteOAuth2Authentication<TTokenHandler>(config, tokenHandlerLifetime);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTokenHandler>(
            this IServiceCollection services,
            string endpoint,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Scoped)
            where TTokenHandler : class, ITokenHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration { Endpoint = endpoint };
            return services.AddRemoteOAuth2Authentication<TTokenHandler>(config, tokenHandlerLifetime);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            IConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new TokenServiceEndpointsClientConfiguration
            {
                Endpoint = configuration["Endpoint"] ?? throw new InvalidOperationException("No endpoint for TokenService"),
                HttpClient = configuration["HttpClient"],
                Path = configuration["Path"]
            };
            return services.AddRemoteOAuth2Authentication(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            string endpoint,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new TokenServiceEndpointsClientConfiguration { Endpoint = endpoint };
            return services.AddRemoteOAuth2Authentication(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration
            {
                Endpoint = configuration["Endpoint"] ?? throw new InvalidOperationException("No endpoint for TokenService"),
                HttpClient = configuration["HttpClient"],
                Path = configuration["Path"]
            };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            string endpoint,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration { Endpoint = endpoint };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            TokenServiceEndpointsClientConfiguration configuration,
            Func<IServiceProvider, ITokenHandler> tokenHandlerFactory,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
            => services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
                ServiceDescriptor.Scoped<ITokenHandler>(tokenHandlerFactory),
                configuration,
                cacheOptions
            );

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<IServiceProvider, ITokenHandler> tokenHandlerFactory,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration
            {
                Endpoint = configuration["Endpoint"] ?? throw new InvalidOperationException("No endpoint for TokenService"),
                HttpClient = configuration["HttpClient"],
                Path = configuration["Path"]
            };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
                ServiceDescriptor.Scoped<ITokenHandler>(tokenHandlerFactory),
                config,
                cacheOptions
            );
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this IServiceCollection services,
            string endpoint,
            Func<IServiceProvider, ITokenHandler> tokenHandlerFactory,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new TokenServiceEndpointsClientConfiguration { Endpoint = endpoint };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
                ServiceDescriptor.Scoped(tokenHandlerFactory),
                config,
                cacheOptions
            );
        }
    }
}