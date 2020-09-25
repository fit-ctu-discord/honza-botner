using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HonzaBotner.Services;
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
            return serviceCollection;
        }

        public static IServiceCollection AddBotnerServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUsermapInfoService, UserMapInfoService>();
            serviceCollection.AddHttpClient<IUsermapInfoService, UserMapInfoService>();
            serviceCollection.AddScoped<IDiscordRoleManager, DiscordRoleManager>();

            return serviceCollection;
        }
    }
}
