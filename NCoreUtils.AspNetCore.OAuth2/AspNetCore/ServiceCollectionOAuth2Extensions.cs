using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.AspNetCore.Proto;
using NCoreUtils.OAuth2;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace NCoreUtils.AspNetCore
{
    public static class ServiceCollectionOAuth2Extensions
    {
        private static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            ServiceDescriptor tokenHandler,
            IEndpointConfiguration configuration)
        {
            services
                .AddTokenServiceClient(configuration)
                .Add(tokenHandler);
            services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                .AddRemoteOAuth2AuthenticationScheme();
            return services;
        }

        private static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            ServiceDescriptor tokenHandler,
            IEndpointConfiguration configuration)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            services
                .AddTokenServiceClient(configuration)
                .Add(tokenHandler);
            services.AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                .AddCustomRemoteOAuth2AuthenticationScheme<TAuthenticationHandler>();
            return services;
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<TTokenHandler>(
            this IServiceCollection services,
            IEndpointConfiguration configuration,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Singleton)
            where TTokenHandler : class, ITokenHandler
            => services.AddRemoteOAuth2Authentication(
                ServiceDescriptor.Describe(typeof(ITokenHandler), typeof(TTokenHandler), tokenHandlerLifetime),
                configuration
            );

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            IEndpointConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var handlers = new List<ITokenHandler>(4);
            if (tokenHandlers.HasFlag(TokenHandlers.Bearer))
            {
                handlers.Add(new BearerTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Cookie))
            {
                handlers.Add(new CookieTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Query))
            {
                handlers.Add(new QueryTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Form))
            {
                handlers.Add(new FormTokenHandler());
            }
            var tokenHandler = handlers.Count == 1 ? handlers[0] : new CompositeTokenHandler(handlers);
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            return services.AddRemoteOAuth2Authentication(
                ServiceDescriptor.Singleton<ITokenHandler>(tokenHandler),
                configuration
            );
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            IEndpointConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var handlers = new List<ITokenHandler>(4);
            if (tokenHandlers.HasFlag(TokenHandlers.Bearer))
            {
                handlers.Add(new BearerTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Cookie))
            {
                handlers.Add(new CookieTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Query))
            {
                handlers.Add(new QueryTokenHandler());
            }
            if (tokenHandlers.HasFlag(TokenHandlers.Form))
            {
                handlers.Add(new FormTokenHandler());
            }
            var tokenHandler = handlers.Count == 1 ? handlers[0] : new CompositeTokenHandler(handlers);
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
                ServiceDescriptor.Singleton<ITokenHandler>(tokenHandler),
                configuration
            );
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<TTokenHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Singleton)
            where TTokenHandler : class, ITokenHandler
        {
            var config = new EndpointConfiguration();
            configuration.Bind(config);
            return services.AddRemoteOAuth2Authentication<TTokenHandler>(config, tokenHandlerLifetime);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication<TTokenHandler>(
            this IServiceCollection services,
            string endpoint,
            ServiceLifetime tokenHandlerLifetime = ServiceLifetime.Singleton)
            where TTokenHandler : class, ITokenHandler
        {
            var config = new EndpointConfiguration { Endpoint = endpoint };
            return services.AddRemoteOAuth2Authentication<TTokenHandler>(config, tokenHandlerLifetime);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            IConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new EndpointConfiguration();
            configuration.Bind(config);
            return services.AddRemoteOAuth2Authentication(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            string endpoint,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new EndpointConfiguration { Endpoint = endpoint };
            return services.AddRemoteOAuth2Authentication(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new EndpointConfiguration();
            configuration.Bind(config);
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            string endpoint,
            TokenHandlers tokenHandlers = TokenHandlers.Bearer | TokenHandlers.Cookie | TokenHandlers.Query,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new EndpointConfiguration { Endpoint = endpoint };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandlers, cacheOptions);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            IEndpointConfiguration configuration,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            return services.AddRemoteOAuth2Authentication(ServiceDescriptor.Singleton<ITokenHandler>(tokenHandler), configuration);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            IConfiguration configuration,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new EndpointConfiguration();
            configuration.Bind(config);
            return services.AddRemoteOAuth2Authentication(config, tokenHandler, cacheOptions);
        }

        public static IServiceCollection AddRemoteOAuth2Authentication(
            this IServiceCollection services,
            string endpoint,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
        {
            var config = new EndpointConfiguration { Endpoint = endpoint };
            return services.AddRemoteOAuth2Authentication(config, tokenHandler, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            IEndpointConfiguration configuration,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            if (cacheOptions == IntrospectionCacheOptions.MemoryCache)
            {
                services.TryAddSingleton<IIntrospectionCache, IntrospectionMemoryCache>();
            }
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(ServiceDescriptor.Singleton<ITokenHandler>(tokenHandler), configuration);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            IConfiguration configuration,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new EndpointConfiguration();
            configuration.Bind(config);
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandler, cacheOptions);
        }

        public static IServiceCollection AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(
            this IServiceCollection services,
            string endpoint,
            ITokenHandler tokenHandler,
            IntrospectionCacheOptions cacheOptions = IntrospectionCacheOptions.MemoryCache)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            var config = new EndpointConfiguration { Endpoint = endpoint };
            return services.AddCustomRemoteOAuth2Authentication<TAuthenticationHandler>(config, tokenHandler, cacheOptions);
        }
    }
}