using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HonzaBotner.Services;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotnerServicesOptions(this IServiceCollection serviceCollection,
                   IConfiguration configuration)
        {
            serviceCollection.Configure<DiscordRoleConfig>(configuration.GetSection(DiscordRoleConfig.ConfigName));
            return serviceCollection;
        }

        public static IServiceCollection AddBotnerServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUserMapInfoService, UserMapInfoService>();
            serviceCollection.AddHttpClient<IUserMapInfoService, UserMapInfoService>();
            serviceCollection.AddScoped<IDiscordRoleManager, DiscordRoleManager>();

            return serviceCollection;
        }
    }
}
