using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            serviceCollection.AddScoped<IEmojiCounterService, EmojiCounterService>();
            serviceCollection.AddScoped<IRoleBindingsService, RoleBindingsService>();
            serviceCollection.AddScoped<IWarningService, WarningService>();

            return serviceCollection;
        }
    }
}
