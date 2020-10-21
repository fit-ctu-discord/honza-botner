using System;
using System.Linq;
using HonzaBotner.Discord;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HonzaBotner.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotnerServicesOptions(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.Configure<DiscordRoleConfig>(settings =>
            {
                configuration.GetSection(DiscordRoleConfig.ConfigName).Bind(settings);
            });
            serviceCollection.Configure<CvutConfig>(configuration.GetSection(CvutConfig.ConfigName));
            return serviceCollection;
        }

        public static IServiceCollection AddBotnerServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUsermapInfoService, UserMapInfoService>();
            serviceCollection.AddHttpClient<IUsermapInfoService, UserMapInfoService>();
            serviceCollection.AddScoped<IDiscordRoleManager, DiscordRoleManager>();
            serviceCollection.AddHttpClient<IAuthorizationService, CvutAuthorizationService>();
            serviceCollection.AddScoped<IAuthorizationService, CvutAuthorizationService>();
            serviceCollection.AddTransient<IUrlProvider, AppUrlProvider>();
            serviceCollection.AddTransient<IHashService, Sha256HashService>();

            serviceCollection.AddTransient<IPermissionHandler, AllowAllPermissionHandler>()
                .Decorate<IPermissionHandler, AuthorizePermissionHandler>()
                .Decorate<IPermissionHandler, ModPermissionHandler>();


            return serviceCollection;
        }

        public static IServiceCollection Decorate<TInterface, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
        {
            // grab the existing registration
            var wrappedDescriptor = services.FirstOrDefault(
                s => s.ServiceType == typeof(TInterface));

            // check it&#039;s valid
            if (wrappedDescriptor == null)
                throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered");

            // create the object factory for our decorator type,
            // specifying that we will supply TInterface explicitly
            var objectFactory = ActivatorUtilities.CreateFactory(
                typeof(TDecorator),
                new[] {typeof(TInterface)});

            // replace the existing registration with one
            // that passes an instance of the existing registration
            // to the object factory for the decorator
            services.Replace(ServiceDescriptor.Describe(
                typeof(TInterface),
                s => (TInterface)objectFactory(s, new[] {s.CreateInstance(wrappedDescriptor)}),
                wrappedDescriptor.Lifetime)
            );

            return services;
        }

        private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType!);
        }
    }
}
